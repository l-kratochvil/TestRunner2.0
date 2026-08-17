# ADR-0001: TestRunner.WebApp — Blazor Server web front-end as the migration target for TestRunner.App

- **Status:** Accepted
- **Date:** 2026-08-17
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
("the user can ___") that keeps its UI components, services and models together because they
change together. Pages are thin **composition roots** that arrange components from several
features on one screen; the page↔feature relationship is not 1:1.

Identified features (capabilities):

| Feature             | Capability                                                              |
| ------------------- | ----------------------------------------------------------------------- |
| `TestDiscovery`     | The user can browse and select test suites and test cases.              |
| `TestConfiguration` | The user can configure a test run (versions, station, TestLink options).|
| `TestExecution`     | The user can start a test run and watch its output and results.         |
| `AppSettings`       | The user can view and change persistent application settings.           |

Pages (composition):

- `TestRunnerPage` (`/`) — the main screen, composed of three horizontal bands:
  1. main menu (shared layout component, shown above all pages),
  2. main area: `TestExplorer` (left) + `TestConfigurator` (right),
  3. `TestLog` (bottom).
- `SettingsPage` (`/settings`) — hosts the `AppSettings` feature.

Resulting layout:

```
TestRunner.WebApp/
├─ Components/
│  ├─ App.razor, Routes.razor, _Imports.razor
│  ├─ Layout/    (MainLayout, MainMenu)
│  └─ Pages/     (TestRunnerPage "/", SettingsPage "/settings")
├─ Features/
│  ├─ TestDiscovery/      (Components, Services, Models)
│  ├─ TestConfiguration/  (Components, Services, Models)
│  ├─ TestExecution/      (Components, Services, Models)
│  └─ AppSettings/        (Components, Services, Models)
├─ Shared/       (cross-cutting components/utilities)
├─ wwwroot/
└─ Program.cs
```

The initial commit contains **static UI placeholders only** — no business logic. The default
template samples (`Counter`, `Weather`, `NavMenu`) were removed so that only the feature-based
convention exists in the codebase.

### 5. Deferred decisions

- **Authentication/authorization — deliberately deferred, but mandatory before deployment.**
  Remote access to a tool that starts processes on the test machine is effectively remote code
  execution. The learning phase runs without auth; a dedicated security decision (ADR) is
  required before the app is exposed beyond localhost.
- **Test project** (`TestRunner.WebApp.Tests`, NUnit) — added once the first non-trivial service
  exists.
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
