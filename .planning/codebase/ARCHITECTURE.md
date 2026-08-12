<!-- refreshed: 2026-08-12 -->
# Architecture

**Analysis Date:** 2026-08-12

## System Overview

```text
┌─────────────────────────────────────────────────────────────┐
│                     Editor Tools & Debug                    │
│  UIPageEditor, TransitionInfoEditor, UIPageHierarchyIndicator
│            `Runtime/UIPage.cs` (Editor-only sections)        │
└─────────────────────────────────────────────────────────────┘
         │                                  │
         ▼                                  ▼
┌──────────────────────────────────────────────────────────────┐
│                     UI Page Management                        │
│                   `Runtime/UIPage.cs`                         │
│         (Page Grouping, State, Lifecycle Management)          │
└──────────────────────────────────────────────────────────────┘
         │                                  │
         ├──────────────────────┬───────────┘
         ▼                      ▼
┌────────────────────┐  ┌──────────────────────┐
│  Transition Engine │  │  Transition Config   │
│ `UITransitionFade` │  │  `TransitionInfo`    │
│   (Singleton)      │  │  (Serializable)      │
└────────────────────┘  └──────────────────────┘
         │                      │
         └──────────────────────┘
                  │
                  ▼
┌──────────────────────────────────────────────────────────────┐
│              External Dependencies                            │
│  Cysharp.Threading.Tasks (UniTask)                           │
│  DG.Tweening (DOTween)                                       │
│  UnityEngine.UI (CanvasGroup, Image, RectTransform)          │
└──────────────────────────────────────────────────────────────┘
```

## Component Responsibilities

| Component | Responsibility | File |
|-----------|----------------|------|
| UIPage | Manages individual UI pages, state, registration, grouping, lifecycle | `Runtime/UIPage.cs` |
| UITransitionFade | Singleton fade overlay for fade and cross-fade transitions | `Runtime/UITransitionFade.cs` |
| TransitionInfo | Serializable configuration container for transition properties | `Runtime/TransitionInfo.cs` |
| UIPageEditor | Custom inspector GUI for UIPage in editor | `Runtime/UIPage.cs` (Editor section) |
| TransitionInfoEditor | Custom property drawer for TransitionInfo | `Runtime/TransitionInfo.cs` (Editor section) |
| UIPageHierarchyIndicator | Hierarchy window status badges for UIPage state | `Runtime/UIPage.cs` (Editor section) |

## Pattern Overview

**Overall:** Component-based registry pattern with singleton transition controller

**Key Characteristics:**
- Static registry maintains groups of UIPage instances at runtime
- Async/await patterns for animation sequences using UniTask
- Declarative transition configuration via TransitionInfo
- Lifecycle callback system (IPageShowBegin, IPageShowEnd, IPageHideBegin, IPageHideEnd)
- Editor-time page management and state inspection
- Default page fallback mechanism

## Layers

**Editor Tools Layer:**
- Purpose: Provide editing capabilities, debugging, and state inspection in the editor
- Location: Editor-only code within `Runtime/UIPage.cs` and `Runtime/TransitionInfo.cs` (conditional compilation with `#if UNITY_EDITOR`)
- Contains: Custom editors, property drawers, hierarchy indicators
- Depends on: UIPage, TransitionInfo, UnityEditor
- Used by: Editor UI only (not in runtime)

**Page Management Layer:**
- Purpose: Manage UI page state, grouping, lifecycle, and transitions
- Location: `Runtime/UIPage.cs` (core logic)
- Contains: UIPage component class, state properties, group registry, lifecycle methods
- Depends on: UnityEngine, Cysharp.Threading.Tasks, DG.Tweening, TransitionInfo
- Used by: Game code that needs to show/hide UI screens

**Transition Layer:**
- Purpose: Execute smooth visual transitions between pages
- Location: `Runtime/UITransitionFade.cs`, `Runtime/TransitionInfo.cs`
- Contains: Fade overlay singleton, transition timing, easing configuration
- Depends on: UnityEngine.UI, DG.Tweening, Cysharp.Threading.Tasks
- Used by: UIPage during page transitions

**Configuration Layer:**
- Purpose: Define transition appearance and timing parameters
- Location: `Runtime/TransitionInfo.cs`
- Contains: TransitionInfo class with enum and serializable fields
- Depends on: UnityEngine, DG.Tweening
- Used by: UIPage to read transition configuration

## Data Flow

### Primary Request Path: Opening a Page with Transition

1. **Entry:** Game code calls `UIPage.OpenPage()` or `UIPage.OpenPageAsync()` on target page (`Runtime/UIPage.cs`, line ~130-145)
2. **Validation:** Check if page is already open or transitioning (`Runtime/UIPage.cs`, line ~131-132)
3. **Get Current Page:** Retrieve currently displayed page from registry (`Runtime/UIPage.cs`, line ~265-275)
4. **Transition Initialization:** Call `TransitionPageAsync(current, target)` with transition info (`Runtime/UIPage.cs`, line ~278-288)
5. **Pre-transition Callbacks:** Invoke `IPageShowBegin` and `IPageHideBegin` callbacks (`Runtime/UIPage.cs`, line ~295-299)
6. **Z-order Correction:** Ensure target page appears above current page in hierarchy (`Runtime/UIPage.cs`, line ~301-305)
7. **Animation Dispatch:** Based on `TransitionInfo.m_Type` (`Runtime/UIPage.cs`, line ~308-350):
   - **Fade:** Fade in overlay → hide current → show target → fade out overlay
   - **CrossFade:** Animate both pages' alpha simultaneously
   - **Slide:** Animate both pages' positions using anchored position tweens
8. **Post-transition Callbacks:** Invoke `IPageShowEnd` and `IPageHideEnd` callbacks (`Runtime/UIPage.cs`, line ~352-358)
9. **Cleanup:** Mark pages as no longer transitioning, restore raycast blocking (`Runtime/UIPage.cs`, line ~360-361)

### Static Registry Lookup Path

1. **Register:** During `Awake()`, each UIPage registers itself into `s_PageRegistry` dictionary keyed by `m_GroupName` (`Runtime/UIPage.cs`, line ~80-87)
2. **Query:** Static methods like `GetPage<T>()`, `GetCurrentPage()` look up pages in registry (`Runtime/UIPage.cs`, line ~237-289)
3. **Fallback (Editor):** If not playing, use `FindObjectsByType<UIPage>()` for editor queries (`Runtime/UIPage.cs`, line ~228-236)
4. **Unregister:** During `OnDestroy()`, remove from registry and trigger default page fallback (`Runtime/UIPage.cs`, line ~93-100)

### Fade Transition Sequence

1. **Overlay Creation:** `UITransitionFade.Instance` creates a full-screen Canvas and fade image on first access (`Runtime/UITransitionFade.cs`, line ~11-64)
2. **Fade In:** Animate overlay alpha from 0 → 1 over duration with specified color (`Runtime/UITransitionFade.cs`, line ~77-83)
3. **Page Swap:** Hide current page, show target page (`Runtime/UIPage.cs`, line ~316-318)
4. **Fade Out:** Animate overlay alpha from 1 → 0 (`Runtime/UITransitionFade.cs`, line ~85-91)

**State Management:**
- Page open state: `m_IsOpened` boolean, controls CanvasGroup alpha, interactability, raycasts
- Transition state: `m_IsTransitionPage` boolean prevents concurrent transitions
- Registry: Static `s_PageRegistry` dictionary keyed by group name, values are lists of UIPage instances
- Default page: `m_IsDefault` boolean, used as fallback when current page is destroyed

## Key Abstractions

**Page Group:**
- Purpose: Organize multiple pages into logical sets; only one per group can be open
- Examples: HUD group (health bar, minimap, etc.), Menu group (main menu, pause menu), etc.
- Pattern: String-keyed registry with lists of UIPage instances

**Lifecycle Callbacks:**
- Purpose: Allow custom logic before/after page show/hide animations
- Interfaces: `IPageShowBegin`, `IPageShowEnd`, `IPageHideBegin`, `IPageHideEnd`
- Pattern: Get components on page and invoke interface methods at specific points

**Transition Configuration:**
- Purpose: Encapsulate transition properties (duration, type, colors, easing)
- Type: `TransitionInfo` serializable class
- Pattern: Assigned per page via inspector or overridden at runtime

## Entry Points

**UIPage.OpenPage() (Fire and Forget):**
- Location: `Runtime/UIPage.cs`, line ~130-135
- Triggers: When game code needs to show a page immediately
- Responsibilities: 
  1. Validate page is not already open
  2. Start async transition without waiting
  3. Use cancellation token tied to GameObject destruction

**UIPage.OpenPageAsync() (Awaitable):**
- Location: `Runtime/UIPage.cs`, line ~125-129
- Triggers: When game code needs to wait for page transition to complete
- Responsibilities:
  1. Validate page is not already open
  2. Return UniTask that resolves when transition finishes
  3. Cancel on GameObject destruction

**Static UIPage.GetCurrentPage(string groupName):**
- Location: `Runtime/UIPage.cs`, line ~245-256
- Triggers: Query which page is currently visible in a group
- Returns: First page in group with `m_IsOpened == true`, or null

**Static UIPage.TransitionPageAsync(UIPage target):**
- Location: `Runtime/UIPage.cs`, line ~258-276
- Triggers: Internal entry point for page-to-page transitions
- Responsibilities: Manage full transition lifecycle from current to target page

## Architectural Constraints

- **Single Open Page Per Group:** Only one UIPage per group name can have `m_IsOpened == true` at runtime. Attempting to open another page will close the previous one via transition.

- **CanvasGroup Required:** Each UIPage must have a CanvasGroup component attached (`[RequireComponent(typeof(CanvasGroup))]` at line ~21). UIPage fails at runtime if missing.

- **Editor Mode Registry Fallback:** The registry (`s_PageRegistry`) is cleared when exiting play mode. Editor queries use `FindObjectsByType<>()` instead of registry lookup to support edit-time page toggling.

- **Singleton UITransitionFade:** UITransitionFade is created dynamically if not in scene. Multiple fade transitions are serialized (only one fade in/out can run per frame).

- **No Concurrent Transitions:** Setting `m_IsTransitionPage = true` blocks concurrent transitions on the same page. Attempting to open a transitioning page returns early without error.

- **Cancellation Token Scope:** Most async methods accept `CancellationToken`. If not provided, defaults to `GetCancellationTokenOnDestroy()` tied to the GameObject lifetime.

- **RectTransform Z-order:** The transition logic manually manages sibling index to ensure target page renders above current page during slide/cross-fade transitions.

## Error Handling

**Strategy:** Silent fallback with early returns; no exceptions thrown

**Patterns:**
- **Missing CanvasGroup:** Attempts to get component each call (line ~114); fails silently if missing after first access
- **Page Already Open:** Early return (line ~131) — calling OpenPage on already-open page is no-op
- **Page Transitioning:** Early return (line ~131) — prevents nested transitions
- **Null Registry:** Check `TryGetValue()` before accessing; return empty array if group not found (line ~250)
- **Destroyed Pages:** Registry includes null checks (line ~287) to handle deleted pages without crashing
- **Missing Callbacks:** `GetComponents<>()` returns empty array if no interface implementations exist (line ~82, ~301)

## Cross-Cutting Concerns

**Logging:** None implemented; debug paths commented out (e.g., line ~277)

**Validation:** Minimal — mostly rely on early returns:
- Check page open/transitioning state before proceeding
- Validate registry entries exist before accessing
- Check component existence before using

**Editor/Runtime Separation:** Conditional compilation with `#if UNITY_EDITOR` isolates editor tools:
- UIPageEditor custom inspector (lines ~421-644)
- TransitionInfoEditor property drawer (lines ~452-529)
- UIPageHierarchyIndicator play mode badges (lines ~647-747)
- Static editor callbacks to clear registry on play mode exit (lines ~28-38)

**Async/Cancellation:** All long-running operations use UniTask with cancellation tokens:
- Animations via DOTween with `.WithCancellation(_token)` (line ~189, ~203, etc.)
- Transition sequences fully cancellable if GameObject destroyed

---

*Architecture analysis: 2026-08-12*
