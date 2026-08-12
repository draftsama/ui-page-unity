# Coding Conventions

**Analysis Date:** 2026-08-12

## Naming Patterns

**Files:**
- Classes and scripts follow `PascalCase`: `UIPage.cs`, `TransitionInfo.cs`, `UITransitionFade.cs`
- Avoid underscores in filenames except for generated/meta files

**Functions:**
- Public methods use `PascalCase`: `OpenPage()`, `SetShow()`, `SetDefault()`, `SetGroupName()`
- Private methods use `PascalCase`: `GetCircleTexture()`, `DrawBadge()`, `OnHierarchyGUI()`
- Event handlers use `On` prefix: `OnDestroy()`, `OnInspectorGUI()`, `OnEditorUpdate()`

**Variables:**
- Instance fields (serialized/private) use `m_FieldName`: `m_GroupName`, `m_IsOpened`, `m_CanvasGroup`
- Static fields use `s_FieldName`: `s_PageRegistry`, `s_CircleTexture`, `s_NextRepaintTime`
- Constants use `k_ConstantName`: `k_RepaintInterval`
- Local variables use `camelCase`: `list`, `shouldShow`, `currentDefault`, `hasOpened`
- Parameters use `_parameterName`: `_isShow`, `_token`, `_groupName`, `_milliseconds`

**Types:**
- Classes use `PascalCase`: `UIPage`, `TransitionInfo`, `UITransitionFade`
- Enums use `PascalCase`: `TransitionType`, `PlayModeStateChange`
- Properties expose private fields: `public string GroupName => m_GroupName;`

## Code Style

**Formatting:**
- No specific formatter configured (uses IDE defaults)
- Indentation: 4 spaces (standard C# convention)
- Brace style: Allman style (opening brace on new line)
- Line length: Not strictly enforced, code observed uses reasonable line lengths

**Linting:**
- No explicit linting tool configured
- IDE used: Rider (version 3.0.39) or Visual Studio (2.0.26)
- Follows standard C# naming and formatting conventions

## Import Organization

**Order:**
1. System namespaces: `using System;`, `using System.Linq;`, `using System.Threading;`
2. External libraries: `using Cysharp.Threading.Tasks;`, `using DG.Tweening;`, `using UnityEngine;`
3. Custom namespaces: `using Modules.Utilities;`
4. Aliases: `using Object = UnityEngine.Object;`

**Path Aliases:**
- Not explicitly used in current codebase
- All imports are full namespace paths

**Conditional Compilation:**
```csharp
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif
```
- Editor-only functionality is segregated using preprocessor directives

## Error Handling

**Patterns:**
- Guard clauses (early returns) for null checks: `if (!s_PageRegistry.TryGetValue(...)) return;`
- Null coalescing for fallback values: `var transitionInfo = _overrideTransitionInfo ?? _target.m_TransitionInfo;`
- Safe property access with null-coalescing: `defaultPage?.SetShow(true);`
- No explicit exception throwing observed; uses defensive checks

**Example:**
```csharp
if (!m_CanvasGroup) m_CanvasGroup = GetComponent<CanvasGroup>();
```

## Logging

**Framework:** `console` (uses `Debug.Log` pattern)

**Patterns:**
- Example (commented out in code): `Debug.Log($"TransitionPageAsync current:{current}  - target:{_target}");`
- Minimal logging observed; primarily used for debugging during development

## Comments

**When to Comment:**
- Summary/purpose of public methods
- Complex logic requiring explanation
- State management requirements: "Make sure to call base.Awake() in derived classes"

**JSDoc/TSDoc:**
- Uses `<summary>` XML documentation tags for public methods:
```csharp
/// <summary>
/// Make sure to call base.Awake() in derived classes
/// </summary>
protected virtual void Awake()
```

## Function Design

**Size:**
- Methods are concise and focused on single responsibility
- Longest method is `TransitionPageAsync` (~90 lines) which handles all transition types
- Average method size is 10-30 lines

**Parameters:**
- Parameters prefixed with underscore: `_isShow`, `_token`, `_groupName`
- Default parameters used for optional values: `TransitionInfo _overrideTransitionInfo = null`
- CancellationToken as standard async parameter: `CancellationToken _token = default`

**Return Values:**
- Public methods return `void`, `UniTask`, or `T` (generic types)
- Boolean return for status: `public static bool ResetUIPagesWithoutNotify(string _groupName)`
- Properties use expression-bodied members: `public string GroupName => m_GroupName;`

## Module Design

**Exports:**
- Public classes and interfaces: `UIPage`, `TransitionInfo`, `IPageShowBegin`, `IPageShowEnd`, `IPageHideBegin`, `IPageHideEnd`
- Static utility methods for page management: `GetPage<T>()`, `GetPageByName()`, `GetCurrentPage()`, `GetPages()`
- Singleton pattern for `UITransitionFade.Instance`

**Barrel Files:**
- Not used; each class defined in separate file

**Namespaces:**
- `Modules.Utilities` for runtime code
- `Modules.Utilities.Editor` for editor-only code (separated by `#if UNITY_EDITOR`)

## Async Patterns

**Framework:** UniTask (`Cysharp.Threading.Tasks`)

**Pattern:**
```csharp
public async UniTask OpenPageAsync(TransitionInfo _overrideTransitionInfo = null, CancellationToken _token = default)
{
    if (m_IsOpened || m_IsTransitionPage) return;
    if (_token == default)
        _token = this.GetCancellationTokenOnDestroy();
    await TransitionPageAsync(this, _overrideTransitionInfo, _token);
}
```

**Cancellation Pattern:**
- Uses `this.GetCancellationTokenOnDestroy()` for automatic cleanup on object destruction
- Cancellation token is standard parameter on async methods

## Unity-Specific Patterns

**MonoBehaviour Lifecycle:**
- `Awake()`: Initialize and register in page registry
- `OnDestroy()`: Cleanup and restore default page
- Virtual methods for derived classes: `protected virtual void Awake()`

**Serialization:**
- `[SerializeField]` for private fields that need editor exposure
- `[HideInInspector]` to hide internal tracking fields
- `[Serializable]` for data classes like `TransitionInfo`

**Editor Scripting:**
- Custom inspectors using `[CustomEditor(typeof(T))]`
- Custom property drawers using `[CustomPropertyDrawer(typeof(T))]`
- Editor callbacks: `EditorApplication.playModeStateChanged += ...`
- Hierarchy visualization using `EditorApplication.hierarchyWindowItemOnGUI += ...`

**Animation:**
- DOTween for tweening: `m_CanvasGroup.DOFade(targetAlpha, duration)`
- Animation chaining: `UniTask.WhenAll(...)`
- UI Canvas animations with `CanvasGroup` for fade/interactivity

---

*Convention analysis: 2026-08-12*
