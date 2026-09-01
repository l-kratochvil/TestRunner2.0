# ADR-0001: Initial architecture of TestRunner.WebApp — Blazor Server web front-end as the migration target for TestRunner.App

- **Status:** Accepted
- **Date:** 2026-08-17 (updated 2026-08-26: `AppLogging` extracted from `TestExecution`;
  `TestResultInspection` and `TestResultReporting` split off from `TestExecution`; updated
  2026-08-27: logging through `Microsoft.Extensions.Logging`; DevKit.Core added as a
  cross-repository project reference; updated 2026-08-28: ADR-0002 folded into this ADR; updated
  2026-08-28: test selection and `Shared/Stores` added)
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
│  ├─ Logging/   (IAppLogger, IAppLoggerFactory, LogSeverity, LogSources)
│  └─ Stores/    (state shared across features: LocalStorageStoreBase, TestRunStore)
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
  `AppLoggerFilter` lives here, next to `AppLoggerView`: it holds which checkboxes are ticked and
  changes whenever the panel's filtering changes, so it is not a model.

State that **several features** read and write lives in `Shared/Stores` instead, so that a feature
never has to reach into another one to learn what the user chose: `TestExecution` will read the
selection `TestDiscovery` makes, and neither has to know the other exists.

`AppLoggerHub` is not one of them — and not a store at all. A store holds state that is read whole
and replaced whole; the hub is the log itself, where entries are appended and never replaced, and
where appending one hands it to every sink. It stays inside `AppLogging` because it is the substance
of that one capability, built out of `LogEntry` and `IAppLoggerSink`, which live in the slice. Moving
it would make `Shared` depend on a feature, turning the one rule that keeps the slices independent
upside down. What every feature does need is the **write contract**, and that already lives in
`Shared/Logging`.

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
  thin wrapper, the shared state lives in `IAppLoggerHub`. `IAppLogger`, `IAppLoggerFactory`,
  `LogSeverity` and `LogSources` sit in `Shared/Logging` because every feature writes to the log;
  the rest, including `IAppLoggerHub`, stays inside the slice.
- **Memory:** `AppLoggerHub` is a singleton ring buffer of 2 000 entries, **shared by every browser
  connected to the server** — intended, because the tool serves one test machine.
- **UI:** entries reach the panel in ~150 ms batches, because test runner output arrives in bursts
  that would otherwise flood the SignalR circuit with re-renders. The panel offers multi-select
  filters (severity, source; everything shown by default), a "jump to newest" button and the path
  of today's log file. There is deliberately **no "clear"**: the store is shared, and the file is
  the guarantee that nothing is lost.

**The application log is a source of the logging pipeline, not a competitor to it.** `AppLoggerHub`
hands every entry to `DiagnosticsLoggerSink`, which logs it through `ILogger` under the category
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
composition root wires to `IAppLoggerHub.ReportFailure`. That reaches the in-memory buffer and
**only** the buffer, because routing it through the sinks would loop back into the provider that
just failed. A handler attached after the failure already happened is invoked immediately, since
the provider is created with the pipeline, long before the store exists. The wire is explicit in
`InitAppLogging` rather than hidden in a sink: a sink whose `Write` does nothing is not a sink.

The writer catches **every** exception, not only the expected file system ones, and keeps its loop
alive. An unexpected one would otherwise escape the background loop and end it: the log would stop
writing for good, report nothing, and leave its queue growing unbounded — a failure mode
indistinguishable from the outside from a healthy log with nothing to say.

### 6. Test selection: what the user picks, and how it is kept

The test explorer shows the three levels the test assemblies define — suite, fixture, test case —
and a selector next to each. The rules are the ones the console application already taught the
tester: selecting a group selects everything under it, and a group follows the test cases beneath
it.

**Only test cases are ever selected.** `TestRunStore` keeps the **execution paths** of the selected
test cases and nothing else; how a suite or a fixture looks is computed from the test cases under
it. The alternative — keeping whichever nodes the user clicked — reads well until a single test
case is ticked: the rule that a parent follows its children would put the fixture and the suite in
the selection too, and the runner builds its filter by matching each entry against NUnit's full
name, so a suite entry runs the **whole suite**. The user would see one test ticked and hundreds
run. Deriving the parents instead leaves one description of what runs, which cannot disagree with
itself. It also needs nothing from the runner: a filter of test case paths already selects exactly
those test cases.

Identity is the execution path, not the name. The console application compares test entities by
name, which is not unique across fixtures; making identity the same key the runner executes by
removes a whole class of "ran the wrong test" from the start.

The store keeps **paths rather than entities**, so nothing it holds can outlive the tree it came
from. A rediscovered tree brings new instances, and a store holding the old ones would show an
empty tree while claiming a selection. A path that matches nothing simply selects nothing, which is
what a selection made before the test assemblies changed should do.

**Per circuit, remembered in the browser.** Several testers may put a run together at once, so the
store is scoped and neither sees the other's selection move under their hands. What was selected
last is written to local storage through `ProtectedLocalStorage` and read back on the first render
— the browser cannot be reached before that, which is why `LocalStorageStoreBase` separates
`InitializeAsync` from construction. Only the selection and the IDE version are remembered; the
discovered suites are read from the test assemblies on every start and a copy would go stale on the
next commit.

The selector is a `<button role="checkbox">` rather than `<input type="checkbox">`. A checkbox
cannot show a partial selection from markup — `indeterminate` is a DOM property with no attribute —
and it carries state the browser mutates on its own: completing a partly selected group leaves the
model unchanged, so the renderer emits no edit and the box keeps whatever the browser put there
([aspnetcore#56847](https://github.com/dotnet/aspnetcore/issues/56847)). A button holds no state of
its own, takes `aria-checked="mixed"` as an ordinary attribute, and still answers space and enter.
Every rule therefore lives in C#, and the browser is asked for nothing.

`LocalStorageStoreBase` is a base class proven by a single store, which is one fewer than it takes
to know what belongs in it. It is kept because the next store is expected to want exactly this, and
is worth collapsing into `TestRunStore` if none appears.

### 7. Deferred decisions

- **Authentication/authorization — deliberately deferred, but mandatory before deployment.**
  Remote access to a tool that starts processes on the test machine is effectively remote code
  execution. The learning phase runs without auth; a dedicated security decision (ADR) is
  required before the app is exposed beyond localhost.
- **Test project** (`TestRunner.WebApp.Tests`, NUnit) — added together with `AppLogging`, the
  first non-trivial service.
- **Layering** — no separate Application/Domain/Infrastructure projects yet; layers will be
  split out only when migration pressure justifies them.
- **Test discovery** — `TestRunStore` is seeded with a hand-written tree (`SampleTestSuites`) so
  the explorer has something to show. Reading the test assemblies through `INUnitTestRunnerProxy`
  replaces that seed where it is registered, and nothing above the store has to change.
- **Starting a test run** — `TestExecution` reads the selection from the store and hands the
  resolved test cases to the runner; the store is the seam, so neither feature reaches into the
  other.

### 8. Dependency: DevKit.Core as a cross-repository project reference

TestRunner needs small, general-purpose helpers that are not specific to test running: assertions
that stay quiet outside a debugging session, functional result types, and similar. These already
exist in DevKit.Core, a separate repository maintained by the same author, alongside TestRunner and
not derived from it. DevKit.Core is not published to any package feed: it is a plain
multi-targeted library (`net481;net9.0;net9.0-windows;net10.0;net10.0-windows`) built with
`LangVersion=preview`, signed with its own key, and developed in parallel with the applications
consuming it.

Three ways to consume it were considered: a project reference across repository boundaries, a
NuGet package on a local or hosted feed referenced by version, or a git submodule pinning a commit
inside this repository.

`TestRunner.WebApp` references DevKit.Core through a **project reference to a sibling clone**:

```xml
<ProjectReference Include="..\..\DevKit.Core\DevKit.Core\DevKit.Core.csproj" />
```

The reference is declared on `TestRunner.WebApp` alone; other projects reach the library
transitively if they ever need it, so the dependency is stated once, where it is actually used.

Both repositories are under active parallel development. A package feed would put a publish step
between writing a helper and using it, and a submodule would put a commit-and-bump step there; both
costs are paid on every iteration, while the benefit — a reproducible pin — matters only once the
library settles down. **Revisit when the library stabilises:** the natural trigger is DevKit.Core
changing less often than its consumers, or a second consumer or CI pipeline appearing. At that
point this decision is superseded by a package-feed one rather than amended.

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
- The selection is expressed in test case paths alone, so what the tree shows and what the runner
  runs cannot drift apart, and a rediscovered tree needs no reconciliation.

**Negative / risks**

- Blazor Server requires a persistent SignalR connection; the UI is unavailable if the connection
  drops (acceptable for an internal tool on a LAN).
- Server-side state is per-circuit; long-running test runs must be modeled as server-side services
  independent of the circuit lifetime (to survive page reloads) — a known upcoming design task.
- Running without authentication is only acceptable while the app is not exposed; this is an
  explicit, tracked risk.
- Every application log entry now also passes through the Console provider, so the development
  console repeats the panel.
- `IAppLoggerSink` survives with a single real implementation plus a failure channel; it is kept as a
  seam for future destinations and is worth removing if none appear.
- The file provider is reachable from the `AppLogging` panel (for the file path it displays), which
  is a feature reading from `Application/` — tolerated because the panel only asks where the file
  is.
- The selection is per circuit but remembered under one browser key, so two tabs of the same browser
  overwrite each other's remembered selection and the last write is what both see after a reload.
- The remembered selection can only be read after the first render, so the tree is drawn once with
  nothing selected and marked up immediately afterwards.
- A selector that is a button and not a checkbox is ours to keep working: the appearance of all
  three states is drawn in CSS, and only the keyboard behaviour still comes from the platform.
- **The build needs a sibling clone.** `DevKit.Core` must sit next to `TestRunner2.0` in the same
  parent directory. A fresh clone of this repository alone does not build, and neither does CI
  without checking out both. This is the price of the DevKit.Core decision and the first thing a
  newcomer hits; the README says so up front.
- **No version pin on DevKit.Core.** TestRunner always builds against the working tree of
  DevKit.Core, including its uncommitted changes. A breaking change there breaks the build here
  immediately, which is useful while both are young and unhelpful once they are not.
- **Preview language features cross the DevKit.Core boundary.** DevKit.Core uses
  `LangVersion=preview`, and `Debug.SafeFail` is a C# 14 extension member. Consuming it from this
  repository works because both build on the .NET 10 SDK; a future SDK split between the two would
  surface here first.

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
- **DevKit.Core as a NuGet package on a local or hosted feed:** rejected for now — would put a
  publish step between writing a helper and using it, paid on every iteration while both
  repositories are under active parallel development; worth reopening once DevKit.Core settles
  down.
- **DevKit.Core as a git submodule pinning a commit:** rejected for now — would put a
  commit-and-bump step between writing a helper and using it, for the same reason as the package
  feed; a reproducible pin only pays off once the library stabilises.
