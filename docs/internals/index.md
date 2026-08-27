# Internals

The mechanisms of this repository that are not obvious from the code, written for whoever has to
change that code next — human or agent.

This is not a decision record. Decisions and their reasons live in [ADR](../ADR/), and this document
describes how things work **today**, pointing at an ADR instead of repeating its argument. When a
section here outgrows a few paragraphs, it moves into its own file under `docs/internals/` and
leaves a one-line pointer behind, so that this page stays the one place to look.

## Contents

- [JavaScript interop](#javascript-interop)

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
which is why both components keep the wrapper in a nullable field and check it before disposing,
rather than creating one on the way out.