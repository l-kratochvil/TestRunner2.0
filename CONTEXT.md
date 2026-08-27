# TestRunner 2.0 — Context

Shared vocabulary of this repository. Terms are listed as they must be used in code, issues and
documentation; the "not" column names synonyms that are deliberately avoided because they already
mean something else here.

## Domain glossary

| Term             | Meaning                                                                                                                                        | Not                  |
| ---------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- | -------------------- |
| **Test suite**   | A group of test cases as defined by the test assemblies / TestLink.                                                                            |                      |
| **Test case**    | A single test as defined by the test assemblies / TestLink.                                                                                    |                      |
| **Test run**     | One execution of the selected test cases on the test machine.                                                                                  | "test", "build"      |
| **Test result**  | The outcome of a test run (passed / failed), the thing that is uploaded to TestLink.                                                           | "log"                |
| **Log**          | The application activity feed the tester reads: what the application is doing, shown in `AppLoggerView`. Never the test result.                 | "output", "result"   |
| **Log entry**    | One record in the log: timestamp, severity, source, message and optional detail.                                                               | "message", "line"    |
| **Severity**     | How serious an entry is, see `LogSeverity`. A successful outcome is reported as normal progress, there is no `Success`.                        | "level", "log level" |
| **Log source**   | The channel an entry belongs to. A plain string, so that entries arriving from outside can be routed without a mapping; known ones: `LogSources`. | "category", "logger" |
| **Diagnostics**  | Developer-facing records written through `ILogger`. They reach the log file but never the log.                                                  | "log", "debug log"   |
| **Log file**     | The daily file on disk. A superset of the log: it also holds diagnostics and framework records, see ADR-0001.                                   | "log", "test result" |
| **Test station** | The hardware station a test run is executed against.                                                                                           |                      |
| **Options**      | A type registered in DI and bound from configuration that configures one service.                                                              | "settings", "config" |

## Conventions

### Options are immutable

An Options type exposes its values as `{ get; init; }`, so a service's configuration cannot change
underneath it. The instance is what is immutable — `IOptionsMonitor<T>` may still hand out a new one
on reload.

The binder sets `init` properties by reflection, so configuration binding works as before; mutating
an existing instance does not. Configure through `IConfiguration`, in tests too, instead of
`services.Configure<T>(options => …)`. A positional `record` cannot bind — the binder needs a
parameterless constructor.

## Decisions

- [ADR-0001](docs/ADR/ADR-0001-initial.md) — Blazor Server web front-end,
  deployment on the test machine, feature-based structure, the design of the application log, and
  logging through `Microsoft.Extensions.Logging` with the log file as an `ILoggerProvider`.
- [ADR-0002](docs/ADR/ADR-0002-devkit-core-dependency.md) — DevKit.Core consumed as a project
  reference to a sibling clone.

## Beyond the vocabulary

This document defines what the terms mean. How the non-obvious mechanisms behind them work is
described in [docs/internals](docs/internals/index.md), and [README.md](README.md) is the entry
point for a newcomer.
