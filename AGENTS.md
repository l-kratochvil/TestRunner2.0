# Agent skills

## Issue tracker

Issues are tracked as GitHub issues in `i-m-luke/TestRunner2.0` (via the `gh` CLI). See `docs/agents/issue-tracker.md`.

## Triage labels

Default canonical labels (`needs-triage`, `needs-info`, `ready-for-agent`, `ready-for-human`, `wontfix`). See `docs/agents/triage-labels.md`.

## Domain docs

Single-context: one `CONTEXT.md` + `docs/ADR/` at the repo root. See `docs/agents/domain.md`.

**Keep the glossary general.** A term defines a role and the rule that governs it, not the values
that happen to exist today. Enumerating current values (severities, sources, statuses) puts a copy
of the code into prose that goes stale on the next commit and makes the glossary read as a
specification of the present rather than of the concept. Name the type or constant that holds the
values instead, and let the reader follow it.

## Code changes

### Static analysis

Read the analyzer output of every build and drive it to zero: fix errors, warnings and info-level
diagnostics alike.

## Repo layout

### Tooling

`tools/` holds developer tooling supporting work on the repo, kept apart from the shipped application code. Inside it, `tools/scripts/` holds runnable scripts for routine local tasks.
