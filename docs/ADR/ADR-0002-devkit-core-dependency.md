# ADR-0002: DevKit.Core as a cross-repository project reference

- **Status:** Accepted
- **Date:** 2026-08-27
- **Deciders:** l-kratochvil

## Context

TestRunner needs small, general-purpose helpers that are not specific to test running: assertions
that stay quiet outside a debugging session, functional result types, and similar. These already
exist in DevKit.Core, a separate repository maintained by the same author, alongside TestRunner and
not derived from it.

DevKit.Core is not published to any package feed. It is a plain multi-targeted library
(`net481;net9.0;net9.0-windows;net10.0;net10.0-windows`) built with `LangVersion=preview`, signed
with its own key, and developed in parallel with the applications consuming it.

Three ways to consume it were considered:

- a **project reference** across repository boundaries;
- a **NuGet package** on a local or hosted feed, referenced by version;
- a **git submodule** pinning a commit inside this repository.

## Decision

TestRunner.WebApp references DevKit.Core through a **project reference to a sibling clone**:

```xml
<ProjectReference Include="..\..\DevKit.Core\DevKit.Core\DevKit.Core.csproj" />
```

The reference is declared on `TestRunner.WebApp` alone. Other projects reach the library
transitively if they ever need it, so the dependency is stated once, where it is actually used.

Both repositories are under active parallel development. A package feed would put a publish step
between writing a helper and using it, and a submodule would put a commit-and-bump step there; both
costs are paid on every iteration, while the benefit — a reproducible pin — matters only once the
library settles down.

## Consequences

- **The build needs a sibling clone.** `DevKit.Core` must sit next to `TestRunner2.0` in the same
  parent directory. A fresh clone of this repository alone does not build, and neither does CI
  without checking out both. This is the price of the decision and the first thing a newcomer hits;
  the README says so up front.
- **No version pin.** TestRunner always builds against the working tree of DevKit.Core, including
  its uncommitted changes. A breaking change there breaks the build here immediately, which is
  useful while both are young and unhelpful once they are not.
- **Preview language features cross the boundary.** DevKit.Core uses `LangVersion=preview`, and
  `Debug.SafeFail` is a C# 14 extension member. Consuming it from this repository works because
  both build on the .NET 10 SDK; a future SDK split between the two would surface here first.
- **Revisit when the library stabilises.** The natural trigger is DevKit.Core changing less often
  than its consumers, or a second consumer or CI pipeline appearing. At that point this decision is
  superseded by a package-feed one rather than amended.