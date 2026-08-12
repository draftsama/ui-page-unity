# External Integrations

**Analysis Date:** 2026-08-12

## APIs & External Services

**Third-Party Animation Library:**
- DOTween - Tweening and animation library
  - SDK/Client: Bundled at `Assets/Plugins/Demigiant/DOTween/`
  - Integration: Direct C# API usage in `Runtime/UITransitionFade.cs` and `Runtime/UIPage.cs`
  - Auth: None (commercial license via asset purchase)

**Async Task Library:**
- UniTask (Cysharp) - Async/await operations
  - SDK/Client: Git package via `com.cysharp.unitask`
  - Source: https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
  - Auth: None (open-source MIT license)
  - Integration: Direct C# API usage in `Runtime/UIPage.cs`, `Runtime/UITransitionFade.cs`

## Data Storage

**Databases:**
- None - This is a UI module with no persistent data storage
- State is managed in-memory via static registry: `UIPage.s_PageRegistry` in `Runtime/UIPage.cs`

**File Storage:**
- Local filesystem only - No remote file storage integration
- Uses Unity AssetDatabase for scene serialization

**Caching:**
- None - No explicit caching layer
- Runtime caching via static dictionary registry for UIPage instances

## Authentication & Identity

**Auth Provider:**
- None - This is a UI module without authentication requirements
- No user identity or login system

**Notes:**
- If host application requires auth, integration would occur at application level, not in ui-page

## Monitoring & Observability

**Error Tracking:**
- None - No error reporting service integrated
- No Sentry, Rollbar, or similar services

**Logs:**
- Console logging only via `Debug.Log()` (commented out in production code)
- Example: `UIPage.cs` line 270 has commented `Debug.Log()` for transition debugging
- No centralized logging framework (Splunk, DataDog, etc.)

**Performance Monitoring:**
- None - No APM (Application Performance Monitoring) services
- No Profiler integration for external analytics

## CI/CD & Deployment

**Hosting:**
- None - This is a library module
- Deployed as part of host Unity application
- Intended for builds targeting: macOS, iOS, Android, WebGL (inherits from parent project)

**CI Pipeline:**
- None configured - No GitHub Actions, GitLab CI, Jenkins, or similar
- Project uses local build via Unity Editor

**Version Control:**
- Git repository at `Assets/ui-page/.git/`
- Main branch for releases
- Part of larger `UI Page` project at `/Users/draftsama/Works/Unity/UI Page/`

## Environment Configuration

**Required env vars:**
- None - This module has no environment-based configuration
- All configuration is serialized in Inspector or hardcoded defaults

**Hardcoded Defaults:**
- `TransitionInfo.m_Duration` default: 500 milliseconds
- `TransitionInfo.m_Type` default: Not set (enum)
- `TransitionInfo.m_FadeColor` default: `Color.black`
- `TransitionInfo.m_Ease` default: `Ease.InOutQuad`
- `UIPage.m_GroupName` default: "Default"

**Secrets location:**
- None - No secrets or API keys used in this module
- If host application requires secrets, use Unity's ScriptableObjects or ConfigManager

## Webhooks & Callbacks

**Incoming:**
- None - This is a UI module without network capabilities

**Outgoing:**
- None - No HTTP webhooks or event publishing

**Internal Event System:**
- Uses interface-based callbacks for page lifecycle:
  - `IPageShowBegin` - Called before page show animation starts
  - `IPageShowEnd` - Called after page show animation completes
  - `IPageHideBegin` - Called before page hide animation starts
  - `IPageHideEnd` - Called after page hide animation completes
  - Location: `Runtime/UIPage.cs` lines 385-403
  - Usage: Components can implement these interfaces to react to page transitions

## Scene Management

**Scene Persistence:**
- Uses Unity's EditorBuildSettings for scene management
- Scenes are part of parent `UI Page` project, not separate in ui-page module

**Prefab System:**
- Relies on serialized UIPage components on Canvas GameObject
- Dynamic runtime creation in `UITransitionFade.Instance` (singleton pattern) at `Runtime/UITransitionFade.cs` lines 22-65

## Physics & Collision

**Physics System:**
- None - UI module does not use physics
- Uses Canvas-based 2D rect layout system

## Platform-Specific Integrations

**macOS:**
- Native Metal rendering via Universal Render Pipeline
- No macOS-specific APIs used in ui-page module

**iOS/Android:**
- Via parent project configuration
- No mobile-specific code in ui-page

**WebGL:**
- Via parent project configuration
- No WebGL-specific code in ui-page

## Built-in Unity Integrations

**UI Framework:**
- Canvas (RenderMode: ScreenSpaceOverlay for transition overlay)
- CanvasGroup (for alpha blending and input blocking)
- RectTransform (for slide animations)
- Image component (for fade transition overlay color)
- GraphicRaycaster (for input handling)

**Editor Integration:**
- CustomEditor attribute: `UIPageEditor` at `Runtime/UIPage.cs` line 411
- CustomPropertyDrawer: `TransitionInfoEditor` at `Runtime/TransitionInfo.cs` line 39
- HierarchyWindowItemOnGUI callback for visual indicators
- EditorApplication lifecycle hooks

## No External Dependencies

This module is intentionally designed with minimal external dependencies:
- Only depends on DOTween (animation) and UniTask (async)
- No network dependencies
- No cloud services
- No analytics
- No social platform integrations
- No payment systems

---

*Integration audit: 2026-08-12*
