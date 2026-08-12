# UI Page

## What This Is

UI Page is a Unity UI navigation package: a registry-pattern page management system with a singleton fade-transition controller. It lets a game register UI screens ("pages") into named groups, then show/hide/cross-fade/slide between them with async lifecycle callbacks (`IPageShowBegin/End`, `IPageHideBegin/End`). Built on Unity 6000.3.8f1, C# 9.0, DOTween for tweening, UniTask for async. Used internally as a reusable UI package across Unity projects.

## Core Value

Existing consumers of this package must keep working after this milestone — public API (`OpenPage`, `ClosePage`, page lifecycle interfaces) can change freely since nothing has shipped yet, but the *system* must remain reliable: page transitions must not hang, corrupt state, or crash.

## Requirements

### Validated

- ✓ Page registry — pages register into named groups, one open per group — existing
- ✓ Async page transitions (Fade, CrossFade, Slide) via UniTask + DOTween — existing
- ✓ Lifecycle callback interfaces (IPageShowBegin/End, IPageHideBegin/End) — existing
- ✓ Editor tooling for page inspection (hierarchy indicator, custom editors) — existing

### Active

- [ ] Fix: transition overlap edge case — rapid double-call to `TransitionPageAsync()` on same group desyncs `m_IsTransitionPage`
- [ ] Fix: editor play-mode state leak — `m_IsOpened` not reset on static-constructor cleanup after exiting play mode mid-transition
- [ ] Fix: null reference in `SetShow()` when CanvasGroup destroyed externally (stale cached reference)
- [ ] Unit tests for UIPage registry (group registration, default page selection, lookup by name/type) — Unity Test Framework 1.6.0, installed but unused
- [ ] Tests for transition state consistency (cancellation, double-open, open-during-transition)
- [ ] Tests for singleton initialization (UITransitionFade creation/destruction/re-creation)
- [ ] Tests for lifecycle callback ordering and exception behavior
- [ ] Refactor: replace `s_PageRegistry` global static Dictionary with an explicit manager/DI pattern (non-static `UIPageManager`)
- [ ] Refactor: replace `UITransitionFade.Instance` lazy singleton with explicit initialization (boot scene / factory / DI) — remove hardcoded canvas sorting order (1000)
- [ ] Refactor: extract hardcoded magic strings ("Default", "TransitionCanvas", "Container", "Fade") into a `UIConstants.cs` with overrideable config
- [ ] Refactor: move all `#if UNITY_EDITOR` editor code out of `Runtime/` into `Editor/` (currently empty scaffold), consistent `Modules.Utilities.Editor` namespace
- [ ] Add validation to `TransitionInfo` (clamp duration >= 0, validate positions) — currently no validation on transition values
- [ ] Wrap editor reflection-based component replacement in try-catch with error logging (`UIPage.cs:448-449`)
- [ ] Perf: cache `GetComponents<IPageShowBegin>()` etc. at Awake/OnEnable instead of querying fresh each transition
- [ ] Perf: replace timer-based `RepaintHierarchyWindow()` (every 0.1s) with state-change-triggered repaint
- [ ] Perf: avoid `FindObjectsByType` full-scene scan on empty-registry page lookup

### Out of Scope

- New transition types beyond Fade/CrossFade/Slide — not requested, keep scope to fix/test/refactor/perf on existing behavior
- Transition interruption / cancel-and-restart API (`CancelTransition()`) — noted as missing feature in codebase audit but not requested this milestone; revisit later
- `IPageTransitionStart` lifecycle callback (pre-visual-transition hook) — same as above, deferred
- Per-phase timing config (`inDurationPercent`/`outDurationPercent`) for transitions — deferred, not core to bug/test/refactor/perf goals

## Context

- Brownfield Unity package, 3 runtime files (~950 lines total), 0% existing test coverage
- Codebase mapped previously (`.planning/codebase/`: STACK.md, ARCHITECTURE.md, STRUCTURE.md, CONVENTIONS.md, TESTING.md, INTEGRATIONS.md, CONCERNS.md) — CONCERNS.md is the primary source for this milestone's scope
- Source code (`Runtime/`, `Editor/`) was present on disk but had never been committed to git; committed as baseline (edc777b) before starting this work, so all fixes/refactors are diffable against the original behavior
- Unity Test Framework (com.unity.test-framework 1.6.0) is installed but has zero tests currently — this milestone is also the first real usage of it in this repo
- `Editor/` folder exists but is empty — a scaffold for the editor-code extraction refactor

## Constraints

- **Compatibility**: None on public API — package is unreleased/undeployed, so breaking changes to `OpenPage`/`ClosePage`/registry internals are acceptable if they produce a better design
- **Tech stack**: Must stay within Unity 6000.3.8f1 / C# 9.0 / .NET Standard 2.1, DOTween + UniTask as existing dependencies — no new third-party dependencies without strong justification
- **Reliability**: Any refactor to registry/singleton must not regress the 3 known bugs being fixed — fixes and tests should land before/alongside the deeper refactor, not after

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Breaking API changes allowed during refactor | Package not yet deployed/consumed elsewhere; better to fix design now than carry tech debt forward | — Pending |
| Work order: Bugs → Tests → Refactor → Perf | Fix known-bad behavior first, then build a test safety net before touching architecture, so refactor risk is caught by tests | — Pending |
| Commit existing source to git as baseline before refactoring | Runtime/Editor code existed on disk but was never tracked; needed a diffable starting point | ✓ Good |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-08-12 after initialization*
