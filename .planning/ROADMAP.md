# Roadmap: UI Page

## Overview

This is a brownfield cleanup milestone for the UI Page package, not new feature development. The three known bugs (transition desync, editor state leak, stale CanvasGroup reference) get fixed first so the system stops hanging, corrupting state, or crashing. A test safety net then locks in that corrected behavior — registry, transition state, singleton lifecycle, and callback ordering all get automated coverage using Unity Test Framework for the first time in this repo. Only once that net exists does the deeper architecture refactor happen (static registry → explicit manager, lazy singleton → explicit init, magic strings → constants, editor code → `Editor/`, plus validation and error-handling hardening), so any regression the refactor introduces gets caught immediately instead of shipping silently. Performance cleanup (cached component lookups, event-driven repaint, avoided full-scene scans) closes out the milestone last, since it touches the same code the refactor just restructured.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [ ] **Phase 1: Bugfixes** - Fix the three known reliability bugs (transition desync, editor state leak, stale CanvasGroup) so pages don't hang, corrupt state, or crash
- [ ] **Phase 2: Test Safety Net** - Add Unity Test Framework coverage for registry, transition state, singleton lifecycle, and callback behavior before touching architecture
- [ ] **Phase 3: Architecture Refactor** - Replace the static registry and lazy singleton with explicit patterns, extract magic strings, separate editor code, and harden validation/error handling
- [ ] **Phase 4: Performance** - Cache per-transition component lookups, replace timer-based repaint with state-driven repaint, and avoid full-scene scans on populated lookups

## Phase Details

### Phase 1: Bugfixes
**Goal**: Users of the package no longer hit the three known reliability failures — transitions don't desync, stale editor state doesn't leak across play sessions, and destroyed components don't crash the page.
**Mode:** mvp
**Depends on**: Nothing (first phase)
**Requirements**: BUG-01, BUG-02, BUG-03
**Success Criteria** (what must be TRUE):
  1. Rapidly calling `TransitionPageAsync()` twice on the same page group no longer leaves `m_IsTransitionPage` desynced — the page group remains transitionable immediately after, with no stuck/unresponsive state.
  2. Exiting play mode mid-transition and re-entering play mode no longer leaves `m_IsOpened` in a stale state — pages start in a correct, predictable open/closed state on next play.
  3. Calling `SetShow()` after the page's CanvasGroup has been destroyed externally no longer throws an unhandled exception or silently no-ops — it either recovers (re-fetches the component) or fails with a clear, loggable error.
**Plans**: TBD

### Phase 2: Test Safety Net
**Goal**: The registry, transition state machine, singleton lifecycle, and lifecycle callbacks all have automated test coverage, so the upcoming architecture refactor can be verified against a regression baseline instead of manual testing alone.
**Mode:** mvp
**Depends on**: Phase 1 (tests must validate the corrected post-bugfix behavior, not the original buggy behavior)
**Requirements**: TEST-01, TEST-02, TEST-03, TEST-04
**Success Criteria** (what must be TRUE):
  1. Running the Unity Test Framework suite executes and passes tests covering group registration, default page selection, and page lookup by name/type.
  2. The suite includes passing tests for transition state consistency — cancellation mid-transition, double-open on the same page, and opening a page while another transition is in progress (including the double-call scenario fixed in Phase 1).
  3. The suite includes passing tests for `UITransitionFade` singleton lifecycle — creation on first access, behavior after the instance is destroyed, and correct re-creation on next access.
  4. The suite includes passing tests verifying `IPageShowBegin/End` and `IPageHideBegin/End` callback ordering, and confirming a throwing callback doesn't hang or corrupt the transition.
**Plans**: TBD

### Phase 3: Architecture Refactor
**Goal**: The registry and singleton move from implicit global state to explicit, predictable initialization, with configuration extracted from magic strings and editor code cleanly separated from runtime — all verified against the Phase 2 test suite so no behavior regresses.
**Mode:** mvp
**Depends on**: Phase 2 (test safety net must exist before refactoring registry/singleton internals)
**Requirements**: REFAC-01, REFAC-02, REFAC-03, REFAC-04, REFAC-05, REFAC-06
**Success Criteria** (what must be TRUE):
  1. Page registration and lookup go through an explicit, non-static `UIPageManager` instead of the global static `s_PageRegistry` Dictionary, and the full Phase 2 test suite still passes against the new implementation.
  2. `UITransitionFade` requires explicit initialization — no GameObject is implicitly created on first property access, and the canvas sorting order is no longer hardcoded to 1000 — while fade transitions continue to behave identically to before.
  3. The previously hardcoded strings (`"Default"`, `"TransitionCanvas"`, `"Container"`, `"Fade"`) are sourced from a documented, overrideable `UIConstants.cs` instead of being inlined at each call site.
  4. No `#if UNITY_EDITOR` code remains in `Runtime/` — all editor-only logic (custom inspector, property drawer, hierarchy indicator) lives in `Editor/` under a consistent `Modules.Utilities.Editor` namespace, and both editor tooling and runtime builds behave the same as before the move.
  5. `TransitionInfo` clamps/validates its values (duration >= 0, positions within reasonable bounds) instead of silently accepting anything, and the editor's reflection-based component replacement logs an error instead of failing silently when it can't complete.
**Plans**: TBD

### Phase 4: Performance
**Goal**: The known performance bottlenecks (repeated component lookups per transition, timer-driven hierarchy repaint, full-scene scans on populated lookups) are eliminated without changing any observable transition behavior.
**Mode:** mvp
**Depends on**: Phase 3 (perf changes build on the refactored registry/manager structure rather than the old static registry)
**Requirements**: PERF-01, PERF-02, PERF-03
**Success Criteria** (what must be TRUE):
  1. `IPageShowBegin/End` and `IPageHideBegin/End` handlers are resolved once (Awake/OnEnable) and reused on every transition, instead of `GetComponents<>()` being called fresh each time a transition runs.
  2. `UIPageHierarchyIndicator` repaints the hierarchy window only when a page's tracked state actually changes, not on a fixed 0.1s timer, while still reflecting current state accurately during play mode.
  3. `GetCurrentPage()` and equivalent lookups no longer trigger a full-scene `FindObjectsByType` scan when the registry/manager is already populated for that group.
**Plans**: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3 → 4

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Bugfixes | 0/TBD | Not started | - |
| 2. Test Safety Net | 0/TBD | Not started | - |
| 3. Architecture Refactor | 0/TBD | Not started | - |
| 4. Performance | 0/TBD | Not started | - |
