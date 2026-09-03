# Agent skills

## Issue tracker

Issues are tracked as GitHub issues in `i-m-luke/TestRunner2.0` (via the `gh` CLI). Read
`docs/agents/issue-tracker.md` before creating, reading, commenting on, labelling, or closing an
issue or PR.

## Triage labels

Read `docs/agents/triage-labels.md` before applying a triage label — it holds the canonical label
strings.

## Writing docs

Describe the **essence** — what the thing is for and the rule it upholds — never the
implementation. Use as few sentences as the essence needs, optimally one or two. Applies to every
doc you write in this repo: markdown docs, ADRs, and code doc comments alike.

## Domain docs

Domain knowledge lives in `CONTEXT.md` and `docs/ADR/` at the repo root. Read
`docs/agents/domain.md` before exploring the codebase, and when writing a glossary term or an ADR.

## Code changes

### Static analysis

Read the analyzer output of every build and drive it to zero: fix errors, warnings and info-level
diagnostics alike.

## Repo layout

### Tooling

`tools/` holds developer tooling supporting work on the repo, kept apart from the shipped application code. Inside it, `tools/scripts/` holds runnable scripts for routine local tasks.
