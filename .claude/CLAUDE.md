<!-- GSD:project-start source:PROJECT.md -->

## Project

**UI Page**

UI Page is a Unity UI navigation package: a registry-pattern page management system with a singleton fade-transition controller. It lets a game register UI screens ("pages") into named groups, then show/hide/cross-fade/slide between them with async lifecycle callbacks (`IPageShowBegin/End`, `IPageHideBegin/End`). Built on Unity 6000.3.8f1, C# 9.0, DOTween for tweening, UniTask for async. Used internally as a reusable UI package across Unity projects.

**Core Value:** Existing consumers of this package must keep working after this milestone — public API (`OpenPage`, `ClosePage`, page lifecycle interfaces) can change freely since nothing has shipped yet, but the *system* must remain reliable: page transitions must not hang, corrupt state, or crash.

### Constraints

- **Compatibility**: None on public API — package is unreleased/undeployed, so breaking changes to `OpenPage`/`ClosePage`/registry internals are acceptable if they produce a better design
- **Tech stack**: Must stay within Unity 6000.3.8f1 / C# 9.0 / .NET Standard 2.1, DOTween + UniTask as existing dependencies — no new third-party dependencies without strong justification
- **Reliability**: Any refactor to registry/singleton must not regress the 3 known bugs being fixed — fixes and tests should land before/alongside the deeper refactor, not after

<!-- GSD:project-end -->

<!-- GSD:stack-start source:codebase/STACK.md -->

## Technology Stack

## Languages

- C# - Unity scripting language used for all gameplay and UI system code
- YAML - Configuration files in ProjectSettings and Packages manifest

## Runtime

- Unity 6000.3.8f1 (Latest Unity 6 version)
- .NET Standard 2.1 (netstandard2.1)
- Mono runtime
- Standalone macOS (primary development platform)
- Universal Render Pipeline (URP) 17.3.0

## Frameworks

- Unity Engine 6.0 - Game engine and runtime environment
- Unity UI (uGUI) 2.0.0 - UI system for canvas-based UI
- UniTask (Cysharp) - GitHub: https://github.com/Cysharp/UniTask.git - Async/await task library for Unity
- DOTween - Animation tweening library bundled with project
- Input System 1.18.0 - Modern input handling
- Timeline 1.8.10 - Animation timeline editor
- Visual Scripting 1.9.9 - Node-based scripting (available but not used in ui-page)

## Key Dependencies

- `com.cysharp.unitask` - Async task operations in `UIPage.OpenPageAsync()`, `ShowPageAsync()`, `TransitionPageAsync()`
- DOTween (proprietary) - All animation: fade effects, CanvasGroup alpha transitions, RectTransform slide animations
- `com.unity.render-pipelines.universal` 17.3.0 - URP for graphics rendering
- `com.unity.ugui` 2.0.0 - Canvas, CanvasGroup, RectTransform components
- `com.unity.ide.rider` 3.0.39 - JetBrains Rider IDE integration
- `com.unity.ide.visualstudio` 2.0.26 - Visual Studio IDE integration
- `com.unity.test-framework` 1.6.0 - Unit testing framework (installed but not actively used in ui-page)
- `com.unity.ai.navigation` 2.0.10 - NavMesh system (available for larger projects)

## Configuration

- `ProjectSettings/ProjectSettings.asset` - Main project configuration
- `ProjectSettings/ProjectVersion.txt` - Defines Unity 6000.3.8f1
- C# Language Version: 7.3 (supports modern async/await)
- Target Framework: .NET Standard 2.1
- Unsafe Blocks: Disabled
- Assembly Name: Assembly-CSharp
- Platform: Standalone macOS
- Output: `Temp/bin/Debug/` (generated)
- Build Target: macOS x64
- Manifest: `Packages/manifest.json` - Defines all UPM (Unity Package Manager) dependencies
- Lock File: `Packages/packages-lock.json` - Locked versions for reproducible builds

## C# Language Features Used

- Async/await patterns (via UniTask)
- LINQ (`System.Linq`)
- Generics with type constraints
- Reflection (in editor code)
- Custom attributes and property drawers

## Compiler Defines

- `UNITY_6000_3_8` - Unity version specific
- `DOTWEEN` - DOTween animation library
- `UNITASK_DOTWEEN_SUPPORT` - UniTask and DOTween integration
- `CSHARP_7_3_OR_NEWER` - C# 7.3+ features available
- `UNITY_EDITOR` - Editor-only code compilation
- `NET_STANDARD_2_1` - .NET Standard 2.1 support

## Platform Requirements

- macOS (tested on macOS with Apple Silicon/Intel)
- Unity Hub with Unity 6000.3.8f1 installed
- Visual Studio Code, Rider, or Visual Studio 2022+ for IDE support
- .NET 6.0+ SDK recommended for IDE tooling (though Unity bundles mono)
- macOS 10.13+ (minimum for macOS build target)
- At least 2GB RAM recommended
- GPU with basic OpenGL/Metal support

<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->

## Conventions

## Naming Patterns

- Classes and scripts follow `PascalCase`: `UIPage.cs`, `TransitionInfo.cs`, `UITransitionFade.cs`
- Avoid underscores in filenames except for generated/meta files
- Public methods use `PascalCase`: `OpenPage()`, `SetShow()`, `SetDefault()`, `SetGroupName()`
- Private methods use `PascalCase`: `GetCircleTexture()`, `DrawBadge()`, `OnHierarchyGUI()`
- Event handlers use `On` prefix: `OnDestroy()`, `OnInspectorGUI()`, `OnEditorUpdate()`
- Instance fields (serialized/private) use `m_FieldName`: `m_GroupName`, `m_IsOpened`, `m_CanvasGroup`
- Static fields use `s_FieldName`: `s_PageRegistry`, `s_CircleTexture`, `s_NextRepaintTime`
- Constants use `k_ConstantName`: `k_RepaintInterval`
- Local variables use `camelCase`: `list`, `shouldShow`, `currentDefault`, `hasOpened`
- Parameters use `_parameterName`: `_isShow`, `_token`, `_groupName`, `_milliseconds`
- Classes use `PascalCase`: `UIPage`, `TransitionInfo`, `UITransitionFade`
- Enums use `PascalCase`: `TransitionType`, `PlayModeStateChange`
- Properties expose private fields: `public string GroupName => m_GroupName;`

## Code Style

- No specific formatter configured (uses IDE defaults)
- Indentation: 4 spaces (standard C# convention)
- Brace style: Allman style (opening brace on new line)
- Line length: Not strictly enforced, code observed uses reasonable line lengths
- No explicit linting tool configured
- IDE used: Rider (version 3.0.39) or Visual Studio (2.0.26)
- Follows standard C# naming and formatting conventions

## Import Organization

- Not explicitly used in current codebase
- All imports are full namespace paths

#if UNITY_EDITOR
#endif

- Editor-only functionality is segregated using preprocessor directives

## Error Handling

- Guard clauses (early returns) for null checks: `if (!s_PageRegistry.TryGetValue(...)) return;`
- Null coalescing for fallback values: `var transitionInfo = _overrideTransitionInfo ?? _target.m_TransitionInfo;`
- Safe property access with null-coalescing: `defaultPage?.SetShow(true);`
- No explicit exception throwing observed; uses defensive checks

## Logging

- Example (commented out in code): `Debug.Log($"TransitionPageAsync current:{current}  - target:{_target}");`
- Minimal logging observed; primarily used for debugging during development

## Comments

- Summary/purpose of public methods
- Complex logic requiring explanation
- State management requirements: "Make sure to call base.Awake() in derived classes"
- Uses `<summary>` XML documentation tags for public methods:

## Function Design

- Methods are concise and focused on single responsibility
- Longest method is `TransitionPageAsync` (~90 lines) which handles all transition types
- Average method size is 10-30 lines
- Parameters prefixed with underscore: `_isShow`, `_token`, `_groupName`
- Default parameters used for optional values: `TransitionInfo _overrideTransitionInfo = null`
- CancellationToken as standard async parameter: `CancellationToken _token = default`
- Public methods return `void`, `UniTask`, or `T` (generic types)
- Boolean return for status: `public static bool ResetUIPagesWithoutNotify(string _groupName)`
- Properties use expression-bodied members: `public string GroupName => m_GroupName;`

## Module Design

- Public classes and interfaces: `UIPage`, `TransitionInfo`, `IPageShowBegin`, `IPageShowEnd`, `IPageHideBegin`, `IPageHideEnd`
- Static utility methods for page management: `GetPage<T>()`, `GetPageByName()`, `GetCurrentPage()`, `GetPages()`
- Singleton pattern for `UITransitionFade.Instance`
- Not used; each class defined in separate file
- `Modules.Utilities` for runtime code
- `Modules.Utilities.Editor` for editor-only code (separated by `#if UNITY_EDITOR`)

## Async Patterns

- Uses `this.GetCancellationTokenOnDestroy()` for automatic cleanup on object destruction
- Cancellation token is standard parameter on async methods

## Unity-Specific Patterns

- `Awake()`: Initialize and register in page registry
- `OnDestroy()`: Cleanup and restore default page
- Virtual methods for derived classes: `protected virtual void Awake()`
- `[SerializeField]` for private fields that need editor exposure
- `[HideInInspector]` to hide internal tracking fields
- `[Serializable]` for data classes like `TransitionInfo`
- Custom inspectors using `[CustomEditor(typeof(T))]`
- Custom property drawers using `[CustomPropertyDrawer(typeof(T))]`
- Editor callbacks: `EditorApplication.playModeStateChanged += ...`
- Hierarchy visualization using `EditorApplication.hierarchyWindowItemOnGUI += ...`
- DOTween for tweening: `m_CanvasGroup.DOFade(targetAlpha, duration)`
- Animation chaining: `UniTask.WhenAll(...)`
- UI Canvas animations with `CanvasGroup` for fade/interactivity

<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->

## Architecture

## System Overview

```text

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

- Static registry maintains groups of UIPage instances at runtime
- Async/await patterns for animation sequences using UniTask
- Declarative transition configuration via TransitionInfo
- Lifecycle callback system (IPageShowBegin, IPageShowEnd, IPageHideBegin, IPageHideEnd)
- Editor-time page management and state inspection
- Default page fallback mechanism

## Layers

- Purpose: Provide editing capabilities, debugging, and state inspection in the editor
- Location: Editor-only code within `Runtime/UIPage.cs` and `Runtime/TransitionInfo.cs` (conditional compilation with `#if UNITY_EDITOR`)
- Contains: Custom editors, property drawers, hierarchy indicators
- Depends on: UIPage, TransitionInfo, UnityEditor
- Used by: Editor UI only (not in runtime)
- Purpose: Manage UI page state, grouping, lifecycle, and transitions
- Location: `Runtime/UIPage.cs` (core logic)
- Contains: UIPage component class, state properties, group registry, lifecycle methods
- Depends on: UnityEngine, Cysharp.Threading.Tasks, DG.Tweening, TransitionInfo
- Used by: Game code that needs to show/hide UI screens
- Purpose: Execute smooth visual transitions between pages
- Location: `Runtime/UITransitionFade.cs`, `Runtime/TransitionInfo.cs`
- Contains: Fade overlay singleton, transition timing, easing configuration
- Depends on: UnityEngine.UI, DG.Tweening, Cysharp.Threading.Tasks
- Used by: UIPage during page transitions
- Purpose: Define transition appearance and timing parameters
- Location: `Runtime/TransitionInfo.cs`
- Contains: TransitionInfo class with enum and serializable fields
- Depends on: UnityEngine, DG.Tweening
- Used by: UIPage to read transition configuration

## Data Flow

### Primary Request Path: Opening a Page with Transition

### Static Registry Lookup Path

### Fade Transition Sequence

- Page open state: `m_IsOpened` boolean, controls CanvasGroup alpha, interactability, raycasts
- Transition state: `m_IsTransitionPage` boolean prevents concurrent transitions
- Registry: Static `s_PageRegistry` dictionary keyed by group name, values are lists of UIPage instances
- Default page: `m_IsDefault` boolean, used as fallback when current page is destroyed

## Key Abstractions

- Purpose: Organize multiple pages into logical sets; only one per group can be open
- Examples: HUD group (health bar, minimap, etc.), Menu group (main menu, pause menu), etc.
- Pattern: String-keyed registry with lists of UIPage instances
- Purpose: Allow custom logic before/after page show/hide animations
- Interfaces: `IPageShowBegin`, `IPageShowEnd`, `IPageHideBegin`, `IPageHideEnd`
- Pattern: Get components on page and invoke interface methods at specific points
- Purpose: Encapsulate transition properties (duration, type, colors, easing)
- Type: `TransitionInfo` serializable class
- Pattern: Assigned per page via inspector or overridden at runtime

## Entry Points

- Location: `Runtime/UIPage.cs`, line ~130-135
- Triggers: When game code needs to show a page immediately
- Responsibilities: 
- Location: `Runtime/UIPage.cs`, line ~125-129
- Triggers: When game code needs to wait for page transition to complete
- Responsibilities:
- Location: `Runtime/UIPage.cs`, line ~245-256
- Triggers: Query which page is currently visible in a group
- Returns: First page in group with `m_IsOpened == true`, or null
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

- **Missing CanvasGroup:** Attempts to get component each call (line ~114); fails silently if missing after first access
- **Page Already Open:** Early return (line ~131) — calling OpenPage on already-open page is no-op
- **Page Transitioning:** Early return (line ~131) — prevents nested transitions
- **Null Registry:** Check `TryGetValue()` before accessing; return empty array if group not found (line ~250)
- **Destroyed Pages:** Registry includes null checks (line ~287) to handle deleted pages without crashing
- **Missing Callbacks:** `GetComponents<>()` returns empty array if no interface implementations exist (line ~82, ~301)

## Cross-Cutting Concerns

- Check page open/transitioning state before proceeding
- Validate registry entries exist before accessing
- Check component existence before using
- UIPageEditor custom inspector (lines ~421-644)
- TransitionInfoEditor property drawer (lines ~452-529)
- UIPageHierarchyIndicator play mode badges (lines ~647-747)
- Static editor callbacks to clear registry on play mode exit (lines ~28-38)
- Animations via DOTween with `.WithCancellation(_token)` (line ~189, ~203, etc.)
- Transition sequences fully cancellable if GameObject destroyed

<!-- GSD:architecture-end -->

<!-- GSD:skills-start source:skills/ -->

## Project Skills

No project skills found. Add skills to any of: `.claude/skills/`, `.agents/skills/`, `.cursor/skills/`, `.github/skills/`, or `.codex/skills/` with a `SKILL.md` index file.
<!-- GSD:skills-end -->

<!-- GSD:workflow-start source:GSD defaults -->

## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:

- `/gsd-quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd-debug` for investigation and bug fixing
- `/gsd-execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->

<!-- GSD:profile-start -->

## Developer Profile

> Profile not yet configured. Run `/gsd-profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
