# TestRunner 2.0 — Context

Shared vocabulary of this repository. Terms are listed as they must be used in code, issues and
documentation; the "not" column names synonyms that are deliberately avoided because they already
mean something else here.

## Domain glossary

| Term             | Meaning                                                                                                        | Not                  |
| ---------------- | -------------------------------------------------------------------------------------------------------------- | -------------------- |
| **Test suite**   | A group of test cases as defined by the test assemblies / TestLink.                                            |                      |
| **Test case**    | A single test as defined by the test assemblies / TestLink.                                                    |                      |
| **Test run**     | One execution of the selected test cases on the test machine.                                                  | "test", "build"      |
| **Test result**  | The outcome of a test run (passed / failed), the thing that is uploaded to TestLink.                           | "log"                |
| **Log**          | The application activity feed: what the application is doing, shown in `AppLoggerView`. Never the test result. | "output", "result"   |
| **Log entry**    | One record in the log: timestamp, severity, source, message and optional detail.                               | "message", "line"    |
| **Severity**     | `Debug`, `Info`, `Warning` or `Error`. A successful outcome is reported as `Info`, there is no `Success`.      | "level", "log level" |
| **Log source**   | The channel an entry belongs to: `App`, `TestRun`, `TestLink`. A plain string, see `LogSources`.               | "category", "logger" |
| **Test station** | The hardware station a test run is executed against.                                                           |                      |

## Decisions

- [ADR-0001](docs/ADR/ADR-0001-testrunner-webapp-blazor-server.md) — Blazor Server web front-end,
  deployment on the test machine, feature-based structure, and the design of the application log.
