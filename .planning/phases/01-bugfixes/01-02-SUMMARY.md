---
phase: 01-bugfixes
plan: 02
subsystem: ui
tags: [unity, unitask, dotween, canvasgroup, editor-lifecycle, play-mode]

# Dependency graph
requires:
  - phase: 01-bugfixes plan 01
    provides: "s_TransitioningGroups per-group admission lock and the try/finally in the 2-arg TransitionPageAsync, which this plan's CanvasGroup guard sits inside so the abort path still releases both transition flags and the group lock"
provides:
  - "TryGetCanvasGroup(out CanvasGroup) - single guarded accessor routing every CanvasGroup read in UIPage: recovers a stale reference silently, logs one clickable Debug.LogError and returns false when the component is genuinely destroyed/absent, never mutates the component set"
  - "SetShow/ShowPageAsync/TransitionPageAsync (2-arg) never dereference a null CanvasGroup - m_IsOpened is only written on the success path"
  - "ResetStaticState [RuntimeInitializeOnLoadMethod(SubsystemRegistration)] clearing s_PageRegistry and s_TransitioningGroups before the first scene loads, closing the leak when Enter Play Mode Options disables domain reload"
  - "Awake prunes destroyed carry-over registry entries (RemoveAll) and the hasOpened scan is null-safe, so a destroyed page from a previous session cannot suppress default-page selection"
  - "InitializeEditorHooks/OnPlayModeStateChanged replacing the lazy static constructor - repairs only pages that were actually mid-transition when play mode stopped, in-memory only (no SetDirty/Undo)"
affects: [02-tests, 03-refactor]

# Actuals (#2632)
actuals:
  tokens: 2293
  tasks: 2
  commits: 2

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Self-healing property + guarded accessor pair (m_CanvasGroup property re-fetches, TryGetCanvasGroup logs-and-returns-false) as the standard shape for any cached Unity component reference that a consumer can destroy externally"
    - "[RuntimeInitializeOnLoadMethod(SubsystemRegistration)] for static state reset that must survive Enter Play Mode Options with domain reload disabled, paired with [InitializeOnLoadMethod] (not a lazy static constructor) for editor-only subscription lifecycle"

key-files:
  created: []
  modified:
    - "Runtime/UIPage.cs"

key-decisions:
  - "TryGetCanvasGroup recovery is strictly read-only (re-read GetComponent, never AddComponent/Destroy) per the plan's prohibition against silently mutating a consumer's GameObject"
  - "OnPlayModeStateChanged repair is scoped to wasTransitioning pages only - a page the developer previewed at edit time (SetShow'd via the inspector, not mid-transition) is left untouched, matching the plan's explicit anti-data-loss requirement"

patterns-established:
  - "Guard-clause + local-variable relay pattern for propagating a validated resource (CanvasGroup) through a method body without repeated null checks - established in SetShow/ShowPageAsync/TransitionPageAsync"

requirements-completed: [BUG-02, BUG-03]

coverage:
  - id: D1
    description: "SetShow with a destroyed or removed CanvasGroup logs a Debug.LogError naming the page and its group, leaves m_IsOpened unchanged, and returns without throwing"
    requirement: "BUG-03"
    verification:
      - kind: other
        ref: "awk-extracted source assertion: TryGetCanvasGroup body contains Debug.LogError and neither AddComponent nor Destroy; SetShow's return line precedes its m_IsOpened assignment line (Task 1 <verify><automated> block)"
        status: pass
      - kind: manual_procedural
        ref: "Task 1 <human-check>: remove CanvasGroup in inspector, call SetShow(false), confirm one clickable console error and m_IsOpened unchanged; re-add CanvasGroup, confirm silent recovery"
        status: unknown
    human_judgment: true
    rationale: "No Unity Editor CLI is available in this execution environment to run play mode and observe console output / inspector state visually; the human-check repro requires opening the project in Unity 6000.3.8f1"
  - id: D2
    description: "SetShow recovers silently when the cached CanvasGroup reference is merely stale but a CanvasGroup is still present on the GameObject - re-fetched, no error logged"
    requirement: "BUG-03"
    verification:
      - kind: other
        ref: "Source review: m_CanvasGroup getter uses implicit bool conversion (!m_CanvasGroupCache) to re-fetch via GetComponent<CanvasGroup>() before TryGetCanvasGroup ever logs"
        status: pass
      - kind: manual_procedural
        ref: "Task 1 <human-check> step 4: re-add CanvasGroup after removal, repeat SetShow(false), expect silent success with no console output"
        status: unknown
    human_judgment: true
    rationale: "Same headless-environment limitation as D1 - requires Unity play mode to observe console silence"
  - id: D3
    description: "A transition whose participating page has lost its CanvasGroup aborts with a logged error instead of an unhandled NullReferenceException, and still releases the transition flags and the group lock"
    requirement: "BUG-03"
    verification:
      - kind: other
        ref: "awk-extracted source assertion: TryGetCanvasGroup(out call inside the 2-arg TransitionPageAsync body appears after m_IsTransitionPage = true (i.e. inside the try, covered by Plan 01's finally); callback order unchanged (Task 1 <verify><automated> block)"
        status: pass
      - kind: manual_procedural
        ref: "Task 1 <human-check> step 5: remove target page's CanvasGroup mid-transition, confirm logged error and group remains transitionable afterward"
        status: unknown
    human_judgment: true
    rationale: "Requires Unity play mode to trigger and observe a live transition abort; not executable in this headless environment"
  - id: D4
    description: "Entering play mode always starts with an empty page registry and an empty group-lock set, regardless of Enter Play Mode Options Reload Domain setting"
    requirement: "BUG-02"
    verification:
      - kind: other
        ref: "awk-extracted source assertion: ResetStaticState carries [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] and its body clears both s_PageRegistry and s_TransitioningGroups (Task 2 <verify><automated> block)"
        status: pass
      - kind: manual_procedural
        ref: "Task 2 <human-check> steps 1-2: disable Reload Domain, open page B, exit, re-enter play mode, confirm default page A shows on the second run"
        status: unknown
    human_judgment: true
    rationale: "Requires toggling Enter Play Mode Options and observing two consecutive play sessions in the Unity Editor - not executable headlessly"
  - id: D5
    description: "Awake ignores destroyed carry-over registry entries when deciding whether another page in the group is already open, so the default page still shows on the next play session"
    requirement: "BUG-02"
    verification:
      - kind: other
        ref: "awk-extracted source assertion: Awake body contains RemoveAll and the hasOpened assignment includes a _ != null term (Task 2 <verify><automated> block)"
        status: pass
    human_judgment: false
  - id: D6
    description: "After exiting play mode mid-transition, every page that was mid-transition has m_IsTransitionPage false and m_IsOpened consistent with its CanvasGroup alpha/interactable/blocksRaycasts; pages NOT mid-transition keep their edit-time authored state; the repair never marks the scene/prefab dirty"
    requirement: "BUG-02"
    verification:
      - kind: other
        ref: "awk-extracted source assertion: OnPlayModeStateChanged body contains wasTransitioning and SetShow(page.m_IsDefault), contains neither EditorUtility.SetDirty nor Undo. (Task 2 <verify><automated> block)"
        status: pass
      - kind: manual_procedural
        ref: "Task 2 <human-check> steps 3-5: stop play mode mid-transition and confirm repair; preview a non-default page and confirm it survives a play session with no transition; confirm scene not marked dirty"
        status: unknown
    human_judgment: true
    rationale: "Requires driving a live mid-flight transition and inspecting scene dirty state in the Unity Editor - not executable headlessly"

duration: 30min
completed: 2026-08-12
status: complete
---

# Phase 01 Plan 02: CanvasGroup Guard and Play-Mode State Reset Summary

**Every `CanvasGroup` access in `UIPage` now routes through a self-healing guarded accessor (`TryGetCanvasGroup`), and static page-registry/transition-lock state is unconditionally cleared at the start of every play session via `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`, closing BUG-02 and BUG-03.**

## Performance

- **Duration:** 30 min
- **Started:** 2026-08-12T10:56:00Z
- **Completed:** 2026-08-12T11:26:04Z
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments
- `m_CanvasGroup` is now a self-healing read-only property backed by `m_CanvasGroupCache`: a stale reference is silently re-fetched via `GetComponent<CanvasGroup>()` using Unity's implicit bool conversion (never `== null`), so a destroyed component is correctly treated as missing
- New `TryGetCanvasGroup(out CanvasGroup)` is the single guarded entry point for every CanvasGroup access in the file: it recovers silently on a stale-but-recoverable reference, or logs one clickable `Debug.LogError` naming the page and its group and returns `false` — never creates, attaches, or destroys a component
- `SetShow`, `ShowPageAsync`, and both `TransitionPageAsync` branches (blocksRaycasts setup, CrossFade's `DOFade` pair, Slide's alpha/interactable/blocksRaycasts) now dereference CanvasGroup only through a local obtained from the guard — zero remaining raw `m_CanvasGroup.alpha` / `.interactable` / `.blocksRaycasts` / `.DOFade` occurrences in the file
- `m_IsOpened` is assigned only on `SetShow`'s success path, so a page with no CanvasGroup can never be reported as the group's open page
- The CanvasGroup abort guard inside the 2-arg `TransitionPageAsync` sits inside Plan 01's `try`, after both `m_IsTransitionPage = true` assignments — so Plan 01's `finally` still releases both transition flags and the per-group admission lock on this new abort path
- New `ResetStaticState()` — `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` — clears `s_PageRegistry` and `s_TransitioningGroups` before the first scene loads on every play-mode entry and player build, including when Unity 6's Enter Play Mode Options disable domain reload (the exact configuration that previously leaked stale registry state across sessions)
- `Awake` now prunes destroyed carry-over registry entries (`list.RemoveAll(_ => _ == null)`) and its `hasOpened` scan is null-safe, so a destroyed page from a previous play session can no longer suppress the default page from showing
- The lazy `static UIPage()` constructor is replaced by `InitializeEditorHooks()` (`[InitializeOnLoadMethod]`, runs on every domain load/recompile, unsubscribes then subscribes to avoid double-subscription) and a named `OnPlayModeStateChanged` handler
- `OnPlayModeStateChanged` repairs only pages that were actually mid-transition when play mode stopped (`wasTransitioning` captured before clearing `m_IsTransitionPage`, then `SetShow(page.m_IsDefault)`); a page the developer deliberately previewed at edit time is left untouched. The repair is in-memory only — no `EditorUtility.SetDirty`, no `Undo.` entry
- `OnDestroy`'s default-page fallback now uses `if (defaultPage) defaultPage.SetShow(true);` instead of `defaultPage?.SetShow(true);`, since the null-conditional operator bypasses `UnityEngine.Object`'s destroyed-aware equality overload and would happily invoke a method on an already-destroyed page

## Task Commits

Each task was committed atomically:

1. **Task 1: A destroyed CanvasGroup recovers or fails loudly** - `797a76c` (feat)
2. **Task 2: Static and per-page state cannot survive a play-mode boundary** - `3b93fa8` (feat)

**Plan metadata:** _pending_ (docs: complete plan)

## Files Created/Modified
- `Runtime/UIPage.cs` - self-healing `m_CanvasGroup` property + `TryGetCanvasGroup` guard threaded through `SetShow`/`ShowPageAsync`/`TransitionPageAsync`; new `ResetStaticState` runtime hook; `Awake` carry-over pruning; `InitializeEditorHooks`/`OnPlayModeStateChanged` replacing the lazy static constructor; `OnDestroy` destroyed-object-safe default-page fallback

## Decisions Made
- Recovery inside `TryGetCanvasGroup` is strictly read-only (re-reads `GetComponent`, never adds/destroys a component) per the plan's explicit prohibition against silently mutating a consumer's GameObject as a recovery side effect
- `OnPlayModeStateChanged`'s repair scope is intentionally narrow (`wasTransitioning` only) — widening it to reset every page would erase a developer's deliberately previewed non-default page, which the plan calls out as data loss rather than a fix

## Deviations from Plan

None - plan executed exactly as written. Both tasks' `<action>` steps were followed literally; no Rule 1-4 auto-fixes were needed.

## Issues Encountered
None.

## User Setup Required
None - no external service configuration required.

**Manual verification still owed (see `## Known Stubs` below):** all six `<human-check>` repro steps across both tasks require opening the project in Unity 6000.3.8f1 and driving play mode (toggling Enter Play Mode Options, removing components via the inspector, observing console output and scene dirty state), which this execution environment cannot do. All automated `<verify>` gates (compile + source-structure assertions) pass, including a full re-run of both of Plan 01's automated gates to confirm no regression to the transition state machine this plan builds alongside.

## Known Stubs

None — no stub code, placeholder values, or unwired data paths were introduced. Coverage items D1-D6 above are flagged `human_judgment: true` not because of a stub, but because every plan's `<human-check>` play-mode repro requires the Unity Editor, which this headless environment cannot drive. All automated compile + source-assertion gates for every task are fully green.

## Next Phase Readiness
- `Runtime/UIPage.cs` compiles clean against `Assembly-CSharp.csproj` with `UNITY_EDITOR` defined
- Callback ordering (`IPageShowBegin,IPageHideBegin,IPageShowEnd,IPageHideEnd`) unchanged and verified stable
- Plan 01's try/finally, admission-lock, and callback-ordering gates re-verified green after this plan's edits to the same `TransitionPageAsync` method
- No new third-party dependency introduced; no test-framework scaffolding created (correctly deferred to Phase 2)
- Phase 01 (bugfixes) is now fully executed — both plans complete. Before shipping, a human should run all `<human-check>` play-mode repros from both `01-01-PLAN.md` and `01-02-PLAN.md` in Unity 6000.3.8f1 to close out the `human_judgment: true` coverage items (D1-D6 here, plus D1/D3 from Plan 01)
- Phase 2 (TEST-01..TEST-04) can now build Unity Test Framework coverage against the corrected registry, transition, and CanvasGroup-guard behavior this phase establishes

---
*Phase: 01-bugfixes*
*Completed: 2026-08-12*

## Self-Check: PASSED

- FOUND: `Runtime/UIPage.cs`
- FOUND: `.planning/phases/01-bugfixes/01-02-SUMMARY.md`
- FOUND: commit `797a76c`
- FOUND: commit `3b93fa8`
- FOUND: commit `8726dcb`
