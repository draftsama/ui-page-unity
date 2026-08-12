# Codebase Concerns

**Analysis Date:** 2026-08-12

## Tech Debt

**Global Static Registry for Page Management:**
- Issue: `s_PageRegistry` (static Dictionary) maintains mutable global state for tracking UIPage instances across the application
- Files: `Runtime/UIPage.cs:24`
- Impact: 
  - Thread safety not guaranteed (concurrent access from async transitions could cause crashes)
  - Memory leak risk if pages are destroyed but not properly unregistered
  - State becomes difficult to predict during editor play/stop cycles
  - Debugging state issues across scenes becomes complex
- Fix approach: Replace with dependency injection or a manager service pattern. Create a `UIPageManager` class (non-static) that scenes request and register/deregister pages explicitly

**Singleton Pattern with Lazy Initialization (UITransitionFade):**
- Issue: `UITransitionFade.Instance` property creates GameObject hierarchy on first access with no initialization order control
- Files: `Runtime/UITransitionFade.cs:17-65`
- Impact:
  - Initialization happens at unpredictable times during gameplay
  - If accessed before scene setup completes, creates orphaned UI elements
  - No way to control creation parameters (canvas sorting order is hardcoded to 1000)
  - OnDestroy of the created GameObject would break the singleton
- Fix approach: Use proper initialization in a boot scene or game manager. Make UITransitionFade non-singleton and inject via dependency injection. Or use a factory method to explicitly initialize

**Hardcoded Magic Strings:**
- Issue: Group names ("Default") and GameObject names ("TransitionCanvas", "Container", "Fade") hardcoded throughout
- Files: `Runtime/UIPage.cs:44, 250` and `Runtime/UITransitionFade.cs:28, 34, 43`
- Impact:
  - Renaming UI elements breaks code without warning
  - Difficult to support multiple transitions simultaneously
  - Configuration changes require code edits
- Fix approach: Extract to constants class `UIConstants.cs` with documented default values. Allow overrideable configuration

**Mixed Editor and Runtime Code:**
- Issue: Editor-only code mixed with runtime code using `#if UNITY_EDITOR` preprocessor directives
- Files: `Runtime/UIPage.cs:12-15, 26-41, 203-209, 217-221, 234-237, 245-247, 408-697` and `Runtime/TransitionInfo.cs:5-7, 35-140`
- Impact:
  - Reduces readability by splitting logic across conditional sections
  - Runtime code compiled with different symbols in editor vs. build
  - Makes version control harder to track (editor code changes affect runtime files)
  - Harder to test runtime code in isolation
- Fix approach: Move all editor code (UIPageEditor, UIPageHierarchyIndicator, TransitionInfoEditor) to separate files in `Editor/` folder. Use `namespace Modules.Utilities.Editor` consistently

## Known Bugs

**Transition Overlap Edge Case:**
- Symptoms: If two transitions are triggered rapidly for the same page group, state flags (`m_IsTransitionPage`) can become out of sync, preventing page transitions
- Files: `Runtime/UIPage.cs:288, 291-292, 371-372`
- Trigger: Call `TransitionPageAsync()` twice on the same page group within the same frame
- Workaround: Add delay or manual check before calling `OpenPage()`
- Root cause: `m_IsTransitionPage` flag guards are set but no mutual exclusion prevents overlapping calls to `TransitionPageAsync`

**Editor Play Mode State Leak:**
- Symptoms: After exiting play mode in editor, `m_IsTransitionPage` remains false on pages due to one-time cleanup in static constructor
- Files: `Runtime/UIPage.cs:29-39`
- Trigger: Transition a page in play mode, then exit play mode without completing the transition
- Workaround: Press play again to re-enter play mode and transition normally
- Root cause: Only `m_IsTransitionPage` is reset; `m_IsOpened` state is not restored

**Null Reference in SetShow if CanvasGroup Destroyed:**
- Symptoms: `SetShow()` caches CanvasGroup reference but doesn't verify it still exists
- Files: `Runtime/UIPage.cs:102-112`
- Trigger: Destroy CanvasGroup component from UIPage inspector while game is running
- Workaround: Don't destroy the CanvasGroup component
- Root cause: GetComponent cached on first call; no re-fetch if component is destroyed externally

## Security Considerations

**No Validation of TransitionInfo Values:**
- Risk: Negative duration, invalid color values, or extreme position values can cause undefined behavior
- Files: `Runtime/TransitionInfo.cs:23, 27-28, 30`
- Current mitigation: None
- Recommendations: 
  - Add validation in TransitionInfo constructor or property setters to clamp duration >= 0
  - Validate Vector2 positions are within reasonable screen bounds
  - Add editor-time warnings for invalid values via validation method

**Unsafe Reflection in Editor (Script Replacement):**
- Risk: Using reflection to replace UIPage component (DestroyImmediate + AddComponent) could fail silently
- Files: `Runtime/UIPage.cs:448-449`
- Current mitigation: Type checking before replacement; no error handling
- Recommendations:
  - Wrap in try-catch to log errors if component replacement fails
  - Verify all component values are successfully transferred before destroying old component

## Performance Bottlenecks

**Repeated GetComponent Calls in Transition Loop:**
- Problem: `GetComponents<IPageShowBegin>()` and similar calls made multiple times per transition in `TransitionPageAsync()`
- Files: `Runtime/UIPage.cs:305-309, 365-369` and `Runtime/UIPage.cs:146-150, 156-161`
- Cause: Components queried fresh each time; results not cached
- Impact: O(n) lookup where n = number of components on GameObject; noticeable with many page handlers
- Improvement path: Cache results at Awake/OnEnable: `private IPageShowBegin[] m_ShowBeginHandlers;`

**Full Hierarchy Scan in Editor During Playmode:**
- Problem: `UIPageHierarchyIndicator` calls `EditorApplication.RepaintHierarchyWindow()` every 0.1 seconds while playing
- Files: `Runtime/UIPage.cs:602-614`
- Cause: Visual indicator updates require repainting
- Impact: CPU cost during editor play; could slow inspector responsiveness
- Improvement path: Only repaint when page state changes, not on timer. Use OnDestroy/OnEnable callbacks to trigger repaint

**FindObjectsByType Called on Default Page Lookup:**
- Problem: Static methods like `GetCurrentPage()` do full scene scan if page registry is empty
- Files: `Runtime/UIPage.cs:232-241`
- Cause: Editor-mode fallback scans all objects
- Impact: Noticeable in large scenes with many GameObjects
- Improvement path: Cache results in editor mode or ensure registry is pre-populated

## Fragile Areas

**UIPage Registry Initialization Order:**
- Files: `Runtime/UIPage.cs:24, 70-76`
- Why fragile: 
  - Awake order depends on scene load order and component order in GameObjects
  - Multiple default pages in same group can cause unexpected show behavior (line 79-80)
  - Registry cleared on editor play mode entry but not verified empty before use
- Safe modification:
  - Always explicitly call `SetGroupName()` in Awake if group must change
  - Never rely on implicit default page selection; call `SetShow()` explicitly if needed
  - Add debug logs to verify registry state during initialization
- Test coverage: No unit tests for registry behavior; only manual testing possible

**Transition State Machine:**
- Files: `Runtime/UIPage.cs:267-374`
- Why fragile:
  - Four state flags (m_IsOpened, m_IsTransitionPage, m_CanvasGroup.interactable, m_CanvasGroup.blocksRaycasts) must stay in sync
  - Async operation can be interrupted by token cancellation, leaving flags in inconsistent state
  - No validation that target/current pages exist or haven't been destroyed during transition
- Safe modification:
  - Use atomic state enum instead of multiple flags
  - Always restore state in finally block if transition is cancelled
  - Add null checks before accessing page properties after async delay
- Test coverage: No tests for cancellation scenarios; unclear what happens if page is destroyed mid-transition

**UITransitionFade Singleton State:**
- Files: `Runtime/UITransitionFade.cs:11-23`
- Why fragile:
  - Holds reference to CanvasGroup/Image that could be destroyed if GameObject is deleted
  - Multiple concurrent transitions could conflict if fade timing overlaps
  - No reference counting or lifetime management
- Safe modification:
  - Always check if _instance still exists before using (Could be destroyed by scene unload)
  - Add OnDestroy handler to null out _instance
  - Or refactor to non-singleton pattern
- Test coverage: No tests; concurrent transition behavior untested

## Missing Critical Features

**No Transition Interruption Handling:**
- Problem: While a transition is in progress, there's no safe way to immediately cancel and start a new transition
- Blocks: Can't build UI that allows rapid page switching during transitions
- Impact: Currently must wait for transition to complete
- Recommended fix: Expose `CancelTransition()` method that properly stops active tween and resets state

**No Lifecycle Callbacks for Transition Start:**
- Problem: Only have Begin/End callbacks for Show/Hide, no callback when transition actually starts
- Blocks: Can't prevent user input during transition setup phase (before visual fade/slide begins)
- Impact: Race conditions possible if user taps button right as transition queues
- Recommended fix: Add `IPageTransitionStart` interface called when `TransitionPageAsync()` is entered

**No Configuration for Transition Timing Skew:**
- Problem: Fade transition uses `duration * 0.5` for in/out phases; CrossFade and Slide use full duration
- Blocks: Can't customize how duration is distributed between phases
- Impact: Inconsistent feel across transition types
- Recommended fix: Add phase timing config to TransitionInfo (e.g., inDurationPercent, outDurationPercent)

## Test Coverage Gaps

**No Unit Tests for UIPage Registry:**
- What's not tested: Group registration, default page selection, page lookup by name/type
- Files: `Runtime/UIPage.cs:70-76, 178-196, 200-227, 243-252`
- Risk: Bugs in registry logic could silently corrupt page state without warning
- Priority: High - registry is core to entire system

**No Tests for Transition State Consistency:**
- What's not tested: Flag state after cancellation, edge cases (same page open twice, open during transition)
- Files: `Runtime/UIPage.cs:267-374`
- Risk: Transitions can deadlock if state becomes inconsistent
- Priority: High - transitions are primary user-facing feature

**No Tests for Singleton Initialization:**
- What's not tested: UITransitionFade creation, behavior if instance is destroyed, re-creation on second access
- Files: `Runtime/UITransitionFade.cs:17-65`
- Risk: Unpredictable behavior if singleton breaks
- Priority: Medium - affects fade transitions only

**No Tests for Interface Callbacks:**
- What's not tested: Order of IPageShowBegin/End/IPageHideBegin/End callbacks, behavior if callback throws exception
- Files: `Runtime/UIPage.cs:146-150, 156-161, 305-309, 365-369`
- Risk: Callbacks could cause transition to hang if they throw
- Priority: Medium - common extension point for users

**No Integration Tests for Multi-Group Pages:**
- What's not tested: Multiple page groups operating independently, state isolation between groups
- Files: `Runtime/UIPage.cs` (all static methods)
- Risk: Group isolation assumptions could fail under complex scenarios
- Priority: Low - typical projects use 1-2 groups

---

*Concerns audit: 2026-08-12*
