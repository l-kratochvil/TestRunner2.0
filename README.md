# TestRunner 2.0

A tool for running automated NUnit test suites against a test station and reporting the results to
TestLink. The web front-end (`TestRunner.WebApp`, Blazor Server) runs on the test machine itself and
is reached from a browser.

## Setup

**A sibling clone of DevKit.Core is required.** The solution references it by relative path, so the
two repositories must sit side by side:

```
<parent>
├── TestRunner2.0
└── DevKit.Core
```

Without it the build fails on an unresolvable project reference.

## Where to read on

| Document                                    | What it is for                                                         |
| -------------------------------------------- | ---------------------------------------------------------------------- |
| [CONTEXT.md](CONTEXT.md)                    | The vocabulary of this repository: what each domain term means here.   |
| [docs/ADR](docs/ADR/)                       | Decisions, why they were taken, and what they cost. Dated, not edited. |
| [docs/internals](docs/internals/index.md)   | How the non-obvious mechanisms actually work today.                    |
| [docs/guidelines](docs/guidelines/index.md) | Conventions to follow when writing code in this repository.            |
| [AGENTS.md](AGENTS.md)                      | Working agreements for agents contributing to this repository.         |
