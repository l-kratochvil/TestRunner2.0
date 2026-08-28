# Internals

The mechanisms of this repository that are not obvious from the code, written for whoever has to
change that code next — human or agent.

This is not a decision record. Decisions and their reasons live in [ADR](../ADR/), and this document
describes how things work **today**, pointing at an ADR instead of repeating its argument. When a
section here outgrows a few paragraphs, it moves into its own file under `docs/internals/` and
leaves a one-line pointer behind, so that this page stays the one place to look.

## Contents

- [JavaScript interop](#javascript-interop)
- [The browser bridge](#the-browser-bridge)

## JavaScript interop

Components that need browser behaviour (dragging a splitter, scrolling a panel) keep it in a
collocated `.razor.js` module. `JsModuleInterop` wraps one such module so its path is spelled out
once, and `IJsModuleInteropFactory` builds those wrappers.

### Calls never throw

`InvokeVoidSafeAsync` is the only way to call into a module. It reports failures instead of
propagating them, hence the `Safe` in the name:

- **the browser is gone** — `JSDisconnectedException` when a tab is closed mid-call, or
  `OperationCanceledException` when a circuit shuts down — is ignored in silence. Neither is a
  fault, and a red line in the log every time a tester closes a tab would only teach them to ignore
  the log;
- **anything else** is written to `ILogger` as an error and passed to `Debug.SafeFail`, which stops
  a developer who has a debugger attached and is inert otherwise.

The failure goes to `ILogger`, not to `IAppLogger`: a broken `.razor.js` is developer detail, and
the tester reading the application log can do nothing about it. It therefore reaches the log file
and the debugger, but never the log panel.

Importing the module is part of a call, so a missing or broken `.razor.js` is reported the same
way. A failed import is remembered rather than retried: it is a build or deployment fault, and
retrying would only bury it under repetition.

The consequence for callers is that a failed call is indistinguishable from a successful one. This
is deliberate — a component using JavaScript for presentational behaviour has no recovery to offer
— and it is the reason there is no throwing overload. If a caller ever needs to react, it gets one
then, with a reason on record.

### Why the factory is scoped

`JsModuleInteropFactory` is registered as **scoped**, in `InitServicesExtension.InitSharedServices`.
That follows from `IJSRuntime`, which the framework registers as scoped.

In Blazor Server a scope is **one circuit**: one open browser tab, from the moment it connects until
it goes away. The `IJSRuntime` resolved inside a scope is the one wired to that particular tab —
it could not be otherwise, since it has to know which browser to call into. Components are resolved
from the same scope, so a scoped factory is handed exactly the runtime the component would have
injected itself, and `@inject IJSRuntime` disappears from the components entirely.

A singleton factory would be a bug that DI cannot catch. It would capture the runtime of whichever
circuit happened to build it first and keep it forever, so every other tab would be calling into a
foreign — and, once that first tab closed, a dead — browser session. Scope validation would not
complain, because nothing about the object graph is wrong; only the lifetime is.

The wrappers themselves are **not** registered. A component asks the factory for one, owns it, and
disposes it, following the pattern in the
[Blazor JavaScript interop documentation](https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/).
A component that never rendered never asked for a wrapper, so disposal has nothing to release —
which is why the components keep the wrapper in a nullable field and check it before disposing,
rather than creating one on the way out.

## The browser bridge

`JsModuleInterop` above carries calls one way: .NET asks the browser to do something. The bridge is
the way back, so that a script can say what happened to it.

A module asks for a logger once, naming itself:

```ts
import { createLogger } from "../../Shared/JsInterop/JsBridge.razor.js";

const log = createLogger(import.meta.url);
```

`import.meta.url` is there because JavaScript cannot see who called a function: no reflection
reaches the caller, and the alternative — reading a file name out of `new Error().stack` — parses a
format no standard defines, allocates on every call, and loses the trail inside callbacks. One line
per module buys a name that is always right.

**Import the bridge by that relative path.** The reference to .NET is module state, which only works
while the browser holds a single instance of the file — and the `<ImportMap />` rendered by
`App.razor` is what guarantees it. The map keys on exactly this path and rewrites it to the
fingerprinted URL, so every spelling of it lands on the same module. Reaching the file by a path the
map does not know would load a second copy, with its own `reference` that nobody registers: the
component would register into one instance while every `log` call fell back to the console from the
other.

That rewriting is also why `import.meta.url` arrives carrying a content hash, and why
`toModuleName` strips it along with the `.razor.js` suffix. Left in, a script would appear under a
new name in the log every time its content changed.

### Three ways into the log, and why this is one of them

| Written through           | Reaches                | For                                            |
| ------------------------- | ---------------------- | ---------------------------------------------- |
| `IAppLogger`              | log panel and log file | what the tester needs to know                  |
| `ILogger`                 | log file               | developer detail from the test machine         |
| `createLogger` (this one) | log file               | developer detail from the browser              |

The third exists because the first two run on the server and cannot see a `TypeError` in a script.
It is deliberately **not** wired into `IAppLogger`: a broken script is something the tester can do
nothing about, and `AppLogStore` is shared by every connected browser, so one tab's noise would
reach everyone else's panel.

Records arrive at `BrowserLogger`, which writes them under the single category
`TestRunner.WebApp.Browser` — where the code ran is the one thing that sets them apart from
everything else in the file. Which script they came from is a field of the record instead
(`[{Module}] {Message}`), because a category per script would need configuring per script. The
`TestRunner.` prefix is not decoration: `Logging:File:LogLevel` sets `TestRunner: Debug` against a
`Default: Warning`, so a category outside that prefix would have its `debug` records dropped.

`BrowserDiagnostic` travels as one object rather than four arguments because JavaScript has no
named arguments, and `level`, `module` and `message` are three strings in a row. A level the switch
does not recognize is written as a warning rather than dropped: the record is still evidence, and
the unknown level is itself a bug in the caller.

### Nothing is lost quietly, and nothing loops

Calls made before the bridge registers — or after the component gave the reference up — go to the
browser console. Buffering them would add state for a window that lasts milliseconds; dropping them
outright is how a debugging tool earns distrust.

Every `invokeMethodAsync` carries a `.catch`, and that is load-bearing rather than tidy. The call is
fire-and-forget, so an uncaught rejection would surface as an `unhandledrejection` event — which the
bridge listens for, and would try to report through the very call that just failed. The `catch`
breaks that circle at its source, which is also why its own failure goes to the console instead of
back through `send`.

`register` also starts watching for what nobody reports by hand: `error` and `unhandledrejection`,
both through `addEventListener` rather than `window.onerror`, which is a single slot that assigning
to would evict Blazor's own handler. An `ErrorEvent` names the file it came from and that becomes
the module name; a rejection carries no location at all, so it is logged as `(unknown)`.

Those automatic reports stop after 100. A broken timer raises errors by the thousand and every
report is a round trip over the SignalR circuit, so past some point the reporting freezes the UI
harder than the bug being reported. The count never resets: reporting resumes on a page reload,
which is what replaces the dead page such a storm leaves anyway. **Explicit `log` calls are never
counted** — those stop when the developer who wrote them stops.

### Lifetime

`JsBridge` renders nothing; it sits in `MainLayout` for its lifetime alone, which gives exactly one
bridge per circuit and — through `Routes.razor`'s `DefaultLayout` — covers every routed page,
`/Error` included. `BrowserLogger` is a singleton: it holds a logger and no circuit of its own, so
one instance serves every browser, and each circuit merely wraps it in its own
`DotNetObjectReference`.

On disposal the module is told to `unregister` **first**, and only then is the reference disposed.
The other order would leave the module holding a dead reference whose next call rejects — a
rejection that reaches the handler that reports through that same dead reference.

A reconnect needs no handling: it restores the same circuit, so the reference stays valid. Losing
the circuit for good ends in a page reload, which loads the module again from scratch.
