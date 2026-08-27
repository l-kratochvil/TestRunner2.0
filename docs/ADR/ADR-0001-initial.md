# ADR-0001: Initial architecture of TestRunner.WebApp — Blazor Server web front-end as the migration target for TestRunner.App

- **Status:** Accepted
- **Date:** 2026-08-17 (updated 2026-08-26: `AppLogging` extracted from `TestExecution`;
  `TestResultInspection` and `TestResultReporting` split off from `TestExecution`; updated
  2026-08-27: logging through `Microsoft.Extensions.Logging`)
- **Deciders:** l-kratochvil

## Context

TestRunner.App is a console application (.NET 10, Spectre.Console) that drives automated test runs:
it starts NUnit test runs through a separate proxy process, simulates user input, accesses the local
file system and integrates with TestLink (XML-RPC). Its UI is built as a sequence of "crossroad"
screens with minimal interaction per screen.

We want to migrate the tool to a web UI. The web platform allows a much richer UI, so the screen
flow collapses into essentially two screens: a main test-runner screen and a settings screen.
This project is also a learning vehicle — it must start from a clean, best-practice ("senior level")
project structure rather than an ad-hoc sandbox.

A key architectural fact: the tool's core logic is inherently **local** — it launches processes,
touches the file system and simulates input on the machine where tests execute.

Diagnosing an incident on the test machine needs two halves at the same moment: what the
application was doing, in the tester's language, and what the framework was doing underneath
(circuits dropping, reconnects failing, unhandled exceptions in the pipeline). If the application
keeps its own file sink while the host runs the untouched `Microsoft.Extensions.Logging` pipeline
into Console and Debug providers nobody reads on the test machine, the two halves land in two
places — or, for the framework half, in no place at all. That is what makes an incident
unreadable, so logging is decided here together with the rest of the structure.

## Decision

### 1. Deployment topology: server on the test machine

The web server runs **on the test machine itself**. It owns all test-execution logic (starting
runs, OS access). Users connect remotely with a browser, which acts as a thin UI that displays
state and sends commands. This is the only topology in which the existing local logic can be
migrated without redesigning it into a distributed agent architecture.

### 2. Framework: Blazor Web App, InteractiveServer, global interactivity

- **Template:** Blazor Web App (unified .NET 8+ model), **.NET 10** (`net10.0`), consistent with
  the rest of the solution.
- **Render mode:** `InteractiveServer`, applied **globally**. Components execute on the server
  (full access to .NET/OS APIs of the test machine); the browser holds a thin SignalR connection.
- **No WebAssembly.** Logic must run on the test machine, not in the browser.
- **No MVC controllers for page routing.** Pages are routed by the Blazor `Router` via `@page`
  directives. Controllers may be added later, but only as a REST API layer for data, never for
  page navigation.

### 3. Solution placement and references

- New project `TestRunner.WebApp` at the solution root, added to `TestRunner2.0.slnx`.
- References **only** `TestRunner.Common` (shared models).
- **No reference to `TestRunner.App`** — its console/Spectre concerns must not leak into the web
  app. Logic will be migrated gradually by moving it into `TestRunner.Common` or new service
  layers, not by referencing the console application.

### 4. Project structure: feature-based (vertical slices)

The guiding rule: **a feature is a capability, not a page**. A feature is a vertical slice
("the user can \_\_\_") that keeps its UI components, services and models together because they
change together. Pages are thin **composition roots** that arrange components from several
features on one screen; the page↔feature relationship is not 1:1.

Identified features (capabilities):

| Feature                | Capability                                                               |
| ---------------------- | ------------------------------------------------------------------------ |
| `TestDiscovery`        | The user can browse and select test suites and test cases.               |
| `TestConfiguration`    | The user can configure a test run (versions, station, TestLink options). |
| `TestExecution`        | The user can start and stop a test run.                                  |
| `TestResultInspection` | The user can view the result of a test run.                              |
| `TestResultReporting`  | The user can publish a test run's result to TestLink.                    |
| `AppLogging`           | The user can see and filter what the application is doing.               |
| `AppSettings`          | The user can view and change persistent application settings.            |

`AppLogging` is a capability, not a technical layer: the user watches and filters the activity of
the application. What only that capability needs — the entry model, the in-memory store, the log
file and the panel — lives in the slice. The **write contract** (`IAppLogger`,
`IAppLoggerFactory`, `LogSeverity`, `LogSources`) is the exception: every other feature logs, so
it lives in `Shared/Logging` and the features depend on it instead of on `AppLogging`. Only
`AppLogging` implements it, and `InitAppLogging()` wires the implementation up in DI.

The test result is split across three slices because they change for different reasons:
`TestExecution` **produces** it (starting and stopping the run), `TestResultInspection`
**displays** it, and `TestResultReporting` **sends it out** (TestLink, and later result files,
attachments and notifications). The seam between them is `TestRunResult`, which already lives in
`TestRunner.Common`, so no slice reaches into another's internals.

Pages (composition):

- `TestRunnerPage` (`/`) — the main screen, composed of three panes of equal width:
  1. `TestExplorer` (left) + `TestConfigurator` (middle) + `TestResultView` (right).
- `SettingsPage` (`/settings`) — hosts the `AppSettings` feature.

`MainLayout` wraps every page with the main menu above and the `AppLoggerView` below, so the log is
reachable from every screen. The panel is collapsible; collapsed it shows only the last message
and the number of errors. Between the page and the log sits a `SplitterBar` the user drags to size
the log pane; the size is remembered in local storage and the bar is hidden while the log is
collapsed, since there is nothing to drag.

Resulting layout:

```
TestRunner.WebApp/
├─ Application/
│  ├─ DependencyInjection/  (InitServicesExtension — one Init* method per feature)
│  └─ Logging/              (the log file provider of Microsoft.Extensions.Logging)
├─ Components/
│  ├─ App.razor, Routes.razor, _Imports.razor
│  ├─ Layout/    (MainLayout, MainMenu, SplitterBar, AppLoggerView host)
│  └─ Pages/     (TestRunnerPage "/", SettingsPage "/settings")
├─ Features/
│  ├─ _Imports.razor       (framework usings for feature components)
│  ├─ TestDiscovery/         (Components, Services, Models)
│  ├─ TestConfiguration/     (Components, Services, Models)
│  ├─ TestExecution/         (Components, Services, Models)
│  ├─ TestResultInspection/  (Components, Services, Models)
│  ├─ TestResultReporting/   (Components, Services, Models)
│  ├─ AppLogging/            (Components, Services, Models)
│  └─ AppSettings/           (Components, Services, Models)
├─ Shared/       (cross-cutting code, small by design)
│  ├─ JsModuleInterop.cs  (calls into a component's collocated .razor.js)
│  └─ Logging/   (IAppLogger, IAppLoggerFactory, LogSeverity, LogSources)
├─ wwwroot/
└─ Program.cs
```

Apart from `AppLogging`, the features contain **static UI placeholders only** — no business logic.
The default template samples (`Counter`, `Weather`, `NavMenu`) were removed so that only the
feature-based convention exists in the codebase.

Inside a slice the three folders mean:

- **`Models/`** — domain data and, when it applies, how that data is persisted (serialization or
  mapping attributes). Data shapes, not behaviour: `LogEntry` belongs here.
- **`Services/`** — the behaviour of the capability, including the types configuring it. The DI
  registration itself sits outside the slice, in
  `Application/DependencyInjection/InitServicesExtension`, so that the composition root lists the
  whole application in one place.
- **`Components/`** — the UI, including the plain C# classes that are nothing but UI state.
  `AppLogFilter` lives here, next to `AppLoggerView`: it holds which checkboxes are ticked and
  changes whenever the panel's filtering changes, so it is not a model.

### 5. Application log (`AppLogging`) and the logging pipeline

The log is a **feed for the tester**: what the application is doing, in the tester's language.
Developer diagnostics live in `Microsoft.Extensions.Logging` next to it, and the two meet in the
log file.

- **Entry:** `LogEntry(Timestamp, Severity, Source, Message, Detail?)`, owned by the slice — no
  other feature ever builds one, they call a logger instead.
  `LogSeverity` is `Info | Warning | Error`; success is reported as `Info`. There is no debug
  severity: developer detail is logged through `ILogger<T>` and never reaches the panel.
  `Source` is a plain string (see `LogSources`) so that a channel can be added — or arrive from
  outside — without changing the model.
- **Writing:** `IAppLoggerFactory.CreateLogger(source)` returns an `IAppLogger` bound to that
  source; a default `IAppLogger` for `App` is registered in DI for convenience. The logger is a
  thin wrapper, the shared state lives in `IAppLogStore`. `IAppLogger`, `IAppLoggerFactory`,
  `LogSeverity` and `LogSources` sit in `Shared/Logging` because every feature writes to the log;
  the rest, including `IAppLogStore`, stays inside the slice.
- **Memory:** `AppLogStore` is a singleton ring buffer of 2 000 entries, **shared by every browser
  connected to the server** — intended, because the tool serves one test machine.
- **UI:** entries reach the panel in ~150 ms batches, because test runner output arrives in bursts
  that would otherwise flood the SignalR circuit with re-renders. The panel offers multi-select
  filters (severity, source; everything shown by default), a "jump to newest" button and the path
  of today's log file. There is deliberately **no "clear"**: the store is shared, and the file is
  the guarantee that nothing is lost.

**The application log is a source of the logging pipeline, not a competitor to it.** `AppLogStore`
hands every entry to `BlazorLoggerSink`, which logs it through `ILogger` under the category
`TestRunner.AppLog.<Source>`. Nothing in the application writes to a file directly. The log source
becoming the logger category is the point: it makes one channel filterable on its own through the
standard `Logging:<provider>:LogLevel` configuration, without inventing a filtering mechanism of
our own. The whole arrangement rests on one asymmetry:

> every application log entry is also a pipeline record; not every pipeline record is an
> application log entry.

**The log file is an `ILoggerProvider`.** `FileLoggerProvider` (`[ProviderAlias("File")]`) lives in
`Application/Logging`, not in the `AppLogging` slice: it writes framework records too, so the slice
does not own it. One file per day named `YYYY-MM-DD.log`, the newest files retained, a channel
feeding a background writer, best effort with a one-time failure report. Retention also runs when
the day rolls over, not only at startup, so a long-running application does not accumulate files
indefinitely; files are recognized by parsing their name as a date rather than by a wildcard,
because `Directory` wildcards match more names than they appear to and this code deletes files.

**What lands in the file is configuration, not code.** `Logging:File:LogLevel` is
`Default: Warning` plus `TestRunner: Debug`: the application is verbose, the framework speaks only
when something is wrong. `Information` on the framework would flood the day's file with request and
static-asset noise and make it unreadable exactly when it is needed.

**A broken log file is reported, not swallowed.** `FileLoggerProvider` raises `Failed`, which the
composition root wires to `IAppLogStore.ReportFailure`. That reaches the in-memory buffer and
**only** the buffer, because routing it through the sinks would loop back into the provider that
just failed. A handler attached after the failure already happened is invoked immediately, since
the provider is created with the pipeline, long before the store exists. The wire is explicit in
`InitAppLogging` rather than hidden in a sink: a sink whose `Write` does nothing is not a sink.

The writer catches **every** exception, not only the expected file system ones, and keeps its loop
alive. An unexpected one would otherwise escape the background loop and end it: the log would stop
writing for good, report nothing, and leave its queue growing unbounded — a failure mode
indistinguishable from the outside from a healthy log with nothing to say.

### 6. Deferred decisions

- **Authentication/authorization — deliberately deferred, but mandatory before deployment.**
  Remote access to a tool that starts processes on the test machine is effectively remote code
  execution. The learning phase runs without auth; a dedicated security decision (ADR) is
  required before the app is exposed beyond localhost.
- **Test project** (`TestRunner.WebApp.Tests`, NUnit) — added together with `AppLogging`, the
  first non-trivial service.
- **Layering** — no separate Application/Domain/Infrastructure projects yet; layers will be
  split out only when migration pressure justifies them.

## Consequences

**Positive**

- Server-side execution keeps full access to OS/.NET APIs on the test machine; the existing local
  logic remains migratable without a distributed redesign.
- Feature-based slices keep each capability's UI, logic and models together, which matches how the
  code changes and keeps the migration incremental (one capability at a time).
- Thin composition pages make the two-screen UI explicit while features stay independent.
- Single modern framework (Blazor Web App) with today's default template — good learning value.
- One file holds both halves of an incident, in one timeline; the panel shows only what the tester
  should read.
- Filtering, configuration and provider composition are the platform's, not ours; adding a second
  destination (Seq, event log, a second file) is a registration, not a redesign.

**Negative / risks**

- Blazor Server requires a persistent SignalR connection; the UI is unavailable if the connection
  drops (acceptable for an internal tool on a LAN).
- Server-side state is per-circuit; long-running test runs must be modeled as server-side services
  independent of the circuit lifetime (to survive page reloads) — a known upcoming design task.
- Running without authentication is only acceptable while the app is not exposed; this is an
  explicit, tracked risk.
- Every application log entry now also passes through the Console provider, so the development
  console repeats the panel.
- `IAppLogSink` survives with a single real implementation plus a failure channel; it is kept as a
  seam for future destinations and is worth removing if none appear.
- The file provider is reachable from the `AppLogging` panel (for the file path it displays), which
  is a feature reading from `Application/` — tolerated because the panel only asks where the file
  is.

## Alternatives considered

- **Razor Pages / MVC (controllers + views):** rejected — server-rendered page models without the
  interactive component model; MVC controllers routing pages is a legacy style for a new app.
- **Blazor WebAssembly:** rejected — logic would run in the browser, which cannot start test runs
  on the test machine.
- **Remote web + agent on the test machine:** rejected for now — requires a distributed
  architecture; unnecessary while server-on-test-machine topology is acceptable.
- **A file sink owned by `AppLogging`, next to an independent file provider:** rejected — two files
  on disk drift apart and nobody knows which one to read.
- **`IAppLogger.Debug` kept as a facade that bypasses the store:** rejected — a `Debug` method on
  an interface documented as "writes entries to the application log" that does not write to the
  application log is a lying API.
- **A second, verbose file with `Default: Information`:** considered and dropped — the noise it
  captures (requests, static assets, SignalR frames) costs more in readability and disk than it
  returns in diagnosis. `Logging:File:LogLevel` can be raised without a code change if that turns
  out to be wrong.
- **A true circular buffer inside the file:** rejected — a fixed-length record ring stops the file
  from being chronological plain text, and rewriting the file on every trim is unaffordable. The
  alternative that keeps plain text is segment rotation, which was not worth its complexity once
  the verbose file was dropped.
