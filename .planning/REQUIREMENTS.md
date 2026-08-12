# Requirements: UI Page

**Defined:** 2026-08-12
**Core Value:** Existing consumers of this package must keep working after this milestone — public API can change freely (unreleased), but the system must remain reliable: transitions must not hang, corrupt state, or crash.

## v1 Requirements

Requirements for this cleanup milestone. Each maps to roadmap phases.

### Bugfix

- [x] **BUG-01**: Rapid double-call to `TransitionPageAsync()` on the same page group no longer desyncs `m_IsTransitionPage`, so pages don't get stuck unable to transition
- [x] **BUG-02**: Exiting play mode mid-transition in the editor no longer leaves `m_IsOpened` in a stale state on next play
- [x] **BUG-03**: `SetShow()` no longer throws/no-ops silently when the cached CanvasGroup reference has been destroyed externally

### Testing

- [ ] **TEST-01**: Unit tests cover UIPage registry behavior (group registration, default page selection, lookup by name/type)
- [ ] **TEST-02**: Tests cover transition state consistency (cancellation, double-open, open-during-transition)
- [ ] **TEST-03**: Tests cover UITransitionFade singleton lifecycle (creation, destruction, re-creation)
- [ ] **TEST-04**: Tests cover lifecycle callback ordering and behavior when a callback throws

### Refactor

- [ ] **REFAC-01**: `s_PageRegistry` global static Dictionary is replaced with an explicit, non-static manager pattern
- [ ] **REFAC-02**: `UITransitionFade.Instance` lazy singleton is replaced with explicit initialization (no implicit GameObject creation on first access, no hardcoded canvas sorting order)
- [ ] **REFAC-03**: Hardcoded magic strings ("Default", "TransitionCanvas", "Container", "Fade") are extracted into a documented, overrideable `UIConstants.cs`
- [ ] **REFAC-04**: All `#if UNITY_EDITOR` editor-only code is moved out of `Runtime/` into `Editor/`, under a consistent editor namespace
- [ ] **REFAC-05**: `TransitionInfo` validates its values (duration >= 0, positions within reasonable bounds) instead of accepting anything silently
- [ ] **REFAC-06**: Editor reflection-based component replacement is wrapped in error handling that logs failures instead of failing silently

### Performance

- [ ] **PERF-01**: `GetComponents<IPageShowBegin>()` and equivalent lookups are cached at Awake/OnEnable instead of re-queried on every transition
- [ ] **PERF-02**: `UIPageHierarchyIndicator` repaints on state change instead of on a fixed 0.1s timer
- [ ] **PERF-03**: `GetCurrentPage()` and similar lookups avoid a full-scene `FindObjectsByType` scan when the registry is already populated

## v2 Requirements

Deferred to future release. Tracked but not in current roadmap.

### Transition Control

- **TRANS-01**: `CancelTransition()` API to immediately stop and reset an in-progress transition
- **TRANS-02**: `IPageTransitionStart` lifecycle callback fired when `TransitionPageAsync()` is entered, before the visual transition begins
- **TRANS-03**: Configurable phase timing (`inDurationPercent`/`outDurationPercent`) instead of a fixed duration split

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| New transition types beyond Fade/CrossFade/Slide | Not requested — scope is fix/test/refactor/perf on existing behavior |
| Transition interruption / cancel-and-restart | Deferred to v2 (TRANS-01) |
| Pre-transition lifecycle hook | Deferred to v2 (TRANS-02) |
| Per-phase transition timing config | Deferred to v2 (TRANS-03) |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| BUG-01 | Phase 1 | Complete |
| BUG-02 | Phase 1 | Complete |
| BUG-03 | Phase 1 | Complete |
| TEST-01 | Phase 2 | Pending |
| TEST-02 | Phase 2 | Pending |
| TEST-03 | Phase 2 | Pending |
| TEST-04 | Phase 2 | Pending |
| REFAC-01 | Phase 3 | Pending |
| REFAC-02 | Phase 3 | Pending |
| REFAC-03 | Phase 3 | Pending |
| REFAC-04 | Phase 3 | Pending |
| REFAC-05 | Phase 3 | Pending |
| REFAC-06 | Phase 3 | Pending |
| PERF-01 | Phase 4 | Pending |
| PERF-02 | Phase 4 | Pending |
| PERF-03 | Phase 4 | Pending |

**Coverage:**

- v1 requirements: 16 total
- Mapped to phases: 16 (Phase 1: 3, Phase 2: 4, Phase 3: 6, Phase 4: 3)
- Unmapped: 0 ✓

---
*Requirements defined: 2026-08-12*
*Last updated: 2026-08-12 after roadmap creation (4 phases, 100% coverage)*
