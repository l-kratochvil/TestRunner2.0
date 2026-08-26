# ADR-0001: TestRunner.WebApp — Blazor Server web front-end as the migration target for TestRunner.App

- **Status:** Accepted
- **Date:** 2026-08-17 (updated 2026-08-26: `AppLogging` extracted from `TestExecution`;
  `TestResultInspection` and `TestResultReporting` split off from `TestExecution`)
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
`AppLogging` implements it, and `AddAppLogging()` wires the implementation up in DI.

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
- **`Services/`** — the behaviour of the capability, including the types configuring it
  (`AppLoggingOptions`) and its DI registration.
- **`Components/`** — the UI, including the plain C# classes that are nothing but UI state.
  `AppLogFilter` lives here, next to `AppLoggerView`: it holds which checkboxes are ticked and
  changes whenever the panel's filtering changes, so it is not a model.

### 5. Application log (`AppLogging`)

The log is a **feed for the tester**: what the application is doing, in the tester's language.
Developer-oriented diagnostics may be layered on later (an `ILoggerProvider` bridging
`Microsoft.Extensions.Logging` into the same model), which is why the slice is named after the
activity and not after the audience.

- **Entry:** `LogEntry(Timestamp, Severity, Source, Message, Detail?)`, owned by the slice — no
  other feature ever builds one, they call a logger instead.
  `LogSeverity` is `Debug | Info | Warning | Error`; success is reported as `Info`.
  `Source` is a plain string (`App`, `TestRun`, `TestLink`, see `LogSources`) so that a channel
  can be added — or arrive from outside — without changing the model.
- **Writing:** `IAppLoggerFactory.CreateLogger(source)` returns an `IAppLogger` bound to that
  source; a default `IAppLogger` for `App` is registered in DI for convenience. The logger is a
  thin wrapper, the shared state lives in `IAppLogStore`. `IAppLogger`, `IAppLoggerFactory`,
  `LogSeverity` and `LogSources` sit in `Shared/Logging` because every feature writes to the log;
  the rest, including `IAppLogStore`, stays inside the slice.
- **Memory:** `AppLogStore` is a singleton ring buffer of 2 000 entries, **shared by every browser
  connected to the server** — intended, because the tool serves one test machine.
- **Disk:** every entry, including `Debug`, is mirrored to `%LOCALAPPDATA%\TestRunner.WebApp\logs\app-YYYY-MM-DD.log`
  (one file per day, the 5 newest files are kept). Writing happens on a background loop fed by a
  channel, so logging never blocks the caller on disk I/O. The file is **best effort**: a failure
  is reported once as an `Error` entry and the application keeps running with the memory log only.
- **UI:** entries reach the panel in ~150 ms batches, because test runner output arrives in bursts
  that would otherwise flood the SignalR circuit with re-renders. The panel offers multi-select
  filters (severity, source; `Debug` hidden by default), a "jump to newest" button and the path of
  today's log file. There is deliberately **no "clear"**: the store is shared, and the file is the
  guarantee that nothing is lost.

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

**Negative / risks**

- Blazor Server requires a persistent SignalR connection; the UI is unavailable if the connection
  drops (acceptable for an internal tool on a LAN).
- Server-side state is per-circuit; long-running test runs must be modeled as server-side services
  independent of the circuit lifetime (to survive page reloads) — a known upcoming design task.
- Running without authentication is only acceptable while the app is not exposed; this is an
  explicit, tracked risk.

## Alternatives considered

- **Razor Pages / MVC (controllers + views):** rejected — server-rendered page models without the
  interactive component model; MVC controllers routing pages is a legacy style for a new app.
- **Blazor WebAssembly:** rejected — logic would run in the browser, which cannot start test runs
  on the test machine.
- **Remote web + agent on the test machine:** rejected for now — requires a distributed
  architecture; unnecessary while server-on-test-machine topology is acceptable.
