# Testing Patterns

**Analysis Date:** 2026-08-12

## Test Framework

**Runner:**
- Unity Test Framework (com.unity.test-framework) version 1.6.0
- Status: Installed but not actively used in this codebase
- Config: No test runner configuration files found

**Assertion Library:**
- Unity Test Framework assertions (available but not used)
- NUnit assertions (available via Unity Test Framework dependency)

**Run Commands:**
```bash
# Tests would run through Unity Editor Test Runner window
# Window > General > Test Runner (if tests were present)

# Automated testing via command line (if tests were created)
# unity -runTests -testPlatform editmode
# unity -runTests -testPlatform playmode
```

## Test File Organization

**Location:**
- No test files currently exist in the repository
- Recommended location: `Tests/` or `Tests/Editor/` directories parallel to `Runtime/`
- Current codebase: `Runtime/` for production code, `Editor/` for editor tools (empty)

**Naming:**
- Not established; Unity Test Framework convention: `[Test]` attribute on methods
- File naming would typically: `{ComponentName}Tests.cs` or `{ComponentName}Test.cs`

**Structure:**
```
Assets/ui-page/
├── Runtime/
│   ├── UIPage.cs
│   ├── TransitionInfo.cs
│   └── UITransitionFade.cs
├── Editor/           (currently empty)
└── Tests/            (recommended; not yet created)
    ├── Editor/
    │   └── UIPageEditorTests.cs
    └── Runtime/
        └── UIPageRuntimeTests.cs
```

## Test Structure

**Suite Organization:**
- No test classes currently exist
- Recommended pattern for Unity Test Framework:

```csharp
using NUnit.Framework;
using Modules.Utilities;
using UnityEngine;

public class UIPageTests
{
    private GameObject _testGameObject;
    private UIPage _uiPage;

    [SetUp]
    public void Setup()
    {
        _testGameObject = new GameObject("TestUIPage");
        _uiPage = _testGameObject.AddComponent<UIPage>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(_testGameObject);
    }

    [Test]
    public void SetShow_WithTrue_SetsAlphaToOne()
    {
        // Arrange & Act
        _uiPage.SetShow(true);
        
        // Assert
        Assert.AreEqual(1f, _uiPage.m_CanvasGroup.alpha);
    }
}
```

**Patterns:**
- **Setup pattern**: Use `[SetUp]` to initialize test fixtures; create GameObjects and components
- **Teardown pattern**: Use `[TearDown]` to cleanup; destroy GameObjects created during tests
- **Assertion pattern**: Use NUnit assertions: `Assert.AreEqual()`, `Assert.IsTrue()`, `Assert.IsNull()`, etc.

## Mocking

**Framework:**
- No explicit mocking framework is set up
- Manual mocking could be used: create simple stub classes implementing interfaces
- Recommended: NSubstitute or Moq (if moving to .NET testing; not available in standard Unity setup)

**Patterns (Recommended):**
```csharp
// Stub implementation of page lifecycle interface
public class MockPageShowBegin : IPageShowBegin
{
    public bool OnBeginShowPageCalled { get; private set; }
    
    public void OnBeginShowPage()
    {
        OnBeginShowPageCalled = true;
    }
}
```

**What to Mock:**
- External services/dependencies (databases, network calls)
- Time/animation systems (use `Time.timeScale` or WaitForSeconds alternatives)
- MonoBehaviour lifecycle if isolated logic testing needed

**What NOT to Mock:**
- Unity lifecycle methods (`Awake`, `OnDestroy`)
- Core game logic that's being tested
- UI components (`CanvasGroup`, `RectTransform`)

## Fixtures and Factories

**Test Data:**
- Not established; no test fixtures currently exist
- Recommended pattern:

```csharp
public class UIPageTestFixtures
{
    public static TransitionInfo CreateFadeTransition(int duration = 500, Color? fadeColor = null)
    {
        return new TransitionInfo
        {
            m_Type = TransitionInfo.TransitionType.Fade,
            m_Duration = duration,
            m_FadeColor = fadeColor ?? Color.black
        };
    }

    public static TransitionInfo CreateCrossFadeTransition(int duration = 500)
    {
        return new TransitionInfo
        {
            m_Type = TransitionInfo.TransitionType.CrossFade,
            m_Duration = duration
        };
    }
}
```

**Location:**
- Recommended: `Tests/Editor/Fixtures/` or `Tests/Common/Fixtures/`
- Or inline within test class as helper methods

## Coverage

**Requirements:** 
- None enforced currently
- Recommended target: 70%+ for critical paths (page transitions, state management)

**View Coverage:**
- Coverage tools not integrated
- Enable in Test Runner: Window > General > Test Runner > Enable Coverage

## Test Types

**Unit Tests:**
- **Scope**: Individual methods and properties in isolation
- **Approach**: Test `SetShow()`, `SetDefault()`, `SetGroupName()` with various inputs
- **Example**: Verify that `SetShow(true)` sets `m_IsOpened = true` and enables canvas group
- **Location**: `Tests/Editor/`

**Integration Tests:**
- **Scope**: Multiple components working together (page transitions, registry management)
- **Approach**: Test `TransitionPageAsync()` with actual page objects, verify registry state
- **Example**: Create two pages, transition between them, verify state changes propagate
- **Note**: These should use `[UnityTest]` attribute for coroutine support

**PlayMode Tests:**
- **Scope**: Tests requiring Unity's gameplay loop (animations, async operations)
- **Approach**: Test `OpenPageAsync()`, `ShowPageAsync()` with actual UniTask/DOTween
- **Framework**: Unity Test Framework with play mode runner
- **Location**: `Tests/Runtime/` or `Tests/PlayMode/`

**E2E Tests:**
- **Framework**: Not currently used
- **Alternative**: Manual testing or UI automation frameworks (not integrated)

## Common Patterns

**Async Testing:**
```csharp
[UnityTest]
public IEnumerator OpenPageAsync_SetsIsOpenedToTrue() 
{
    // Arrange
    var uiPage = CreateTestUIPage();
    
    // Act
    var task = uiPage.OpenPageAsync();
    
    // Wait for async operation
    yield return new WaitUntil(() => task.Status != UniTaskStatus.Pending);
    
    // Assert
    Assert.IsTrue(uiPage.IsOpened);
}
```

**Error Testing:**
```csharp
[Test]
public void GetPage_WithNullGroup_ReturnsNull()
{
    // Act
    var page = UIPage.GetPage<UIPage>(null);
    
    // Assert
    Assert.IsNull(page);
}
```

**State Testing (Transitions):**
```csharp
[Test]
public void TransitionPageAsync_UpdatesPageRegistry()
{
    // Arrange
    var currentPage = CreateTestUIPage();
    var targetPage = CreateTestUIPage();
    
    currentPage.SetShow(true);
    Assert.IsTrue(currentPage.IsOpened);
    
    // Act - transition would happen here (in PlayMode test)
    
    // Assert
    Assert.IsFalse(currentPage.IsOpened);
    Assert.IsTrue(targetPage.IsOpened);
}
```

## Critical Areas Needing Tests

Based on codebase analysis, these areas lack test coverage:

**High Priority:**
- `UIPage.TransitionPageAsync()` - Complex state machine with multiple transition types
- `UIPage` registry management - Static dictionary operations in `Awake()` and `OnDestroy()`
- `UITransitionFade.Instance` singleton pattern - Lazy initialization logic
- Transition types: Fade, CrossFade, Slide - Each has distinct animation paths

**Medium Priority:**
- `SetShow()` - State management and CanvasGroup configuration
- `SetDefault()` - Default page switching logic
- `SetGroupName()` - Registry re-registration logic
- Property drawer behavior in `TransitionInfoEditor`

**Setup Guidance:**

1. Create test directories:
```bash
mkdir -p Assets/ui-page/Tests/Editor
mkdir -p Assets/ui-page/Tests/Runtime
```

2. Create Assembly Definition files for tests:
```bash
# Assets/ui-page/Tests/Editor/Modules.Utilities.Editor.Tests.asmdef
# Assets/ui-page/Tests/Runtime/Modules.Utilities.Tests.asmdef
```

3. Begin with simple unit tests for setter methods
4. Progress to integration tests for state management
5. Add play mode tests for async animations

---

*Testing analysis: 2026-08-12*
