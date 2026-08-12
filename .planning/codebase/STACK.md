# Technology Stack

**Analysis Date:** 2026-08-12

## Languages

**Primary:**
- C# - Unity scripting language used for all gameplay and UI system code

**Secondary:**
- YAML - Configuration files in ProjectSettings and Packages manifest

## Runtime

**Environment:**
- Unity 6000.3.8f1 (Latest Unity 6 version)
- .NET Standard 2.1 (netstandard2.1)
- Mono runtime

**Platform Targets:**
- Standalone macOS (primary development platform)
- Universal Render Pipeline (URP) 17.3.0

## Frameworks

**Core:**
- Unity Engine 6.0 - Game engine and runtime environment
- Unity UI (uGUI) 2.0.0 - UI system for canvas-based UI

**Async/Concurrency:**
- UniTask (Cysharp) - GitHub: https://github.com/Cysharp/UniTask.git - Async/await task library for Unity
  - Integrated via git package: `com.cysharp.unitask`
  - Provides lightweight async operations and cancellation token support
  - Location: `Assets/Plugins/Cysharp/UniTask/`

**Animation/Tweening:**
- DOTween - Animation tweening library bundled with project
  - Location: `Assets/Plugins/Demigiant/DOTween/`
  - Used for UI fade, alpha, and position animations
  - C# define: `DOTWEEN` (enabled for project)
  - UniTask integration support enabled: `UNITASK_DOTWEEN_SUPPORT`

**Development Tools:**
- Input System 1.18.0 - Modern input handling
- Timeline 1.8.10 - Animation timeline editor
- Visual Scripting 1.9.9 - Node-based scripting (available but not used in ui-page)

## Key Dependencies

**Critical:**
- `com.cysharp.unitask` - Async task operations in `UIPage.OpenPageAsync()`, `ShowPageAsync()`, `TransitionPageAsync()`
  - GitHub source: https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
- DOTween (proprietary) - All animation: fade effects, CanvasGroup alpha transitions, RectTransform slide animations

**Infrastructure:**
- `com.unity.render-pipelines.universal` 17.3.0 - URP for graphics rendering
- `com.unity.ugui` 2.0.0 - Canvas, CanvasGroup, RectTransform components

**Editor/Development:**
- `com.unity.ide.rider` 3.0.39 - JetBrains Rider IDE integration
- `com.unity.ide.visualstudio` 2.0.26 - Visual Studio IDE integration
- `com.unity.test-framework` 1.6.0 - Unit testing framework (installed but not actively used in ui-page)

**Navigation & AI:**
- `com.unity.ai.navigation` 2.0.10 - NavMesh system (available for larger projects)

## Configuration

**Project Settings Location:**
- `ProjectSettings/ProjectSettings.asset` - Main project configuration
- `ProjectSettings/ProjectVersion.txt` - Defines Unity 6000.3.8f1

**Runtime Configuration:**
- C# Language Version: 7.3 (supports modern async/await)
- Target Framework: .NET Standard 2.1
- Unsafe Blocks: Disabled
- Assembly Name: Assembly-CSharp

**Build Configuration:**
- Platform: Standalone macOS
- Output: `Temp/bin/Debug/` (generated)
- Build Target: macOS x64

**Package Configuration:**
- Manifest: `Packages/manifest.json` - Defines all UPM (Unity Package Manager) dependencies
- Lock File: `Packages/packages-lock.json` - Locked versions for reproducible builds

## C# Language Features Used

- Async/await patterns (via UniTask)
- LINQ (`System.Linq`)
- Generics with type constraints
- Reflection (in editor code)
- Custom attributes and property drawers

## Compiler Defines

**Key defines enabled in this project:**
- `UNITY_6000_3_8` - Unity version specific
- `DOTWEEN` - DOTween animation library
- `UNITASK_DOTWEEN_SUPPORT` - UniTask and DOTween integration
- `CSHARP_7_3_OR_NEWER` - C# 7.3+ features available
- `UNITY_EDITOR` - Editor-only code compilation
- `NET_STANDARD_2_1` - .NET Standard 2.1 support

## Platform Requirements

**Development:**
- macOS (tested on macOS with Apple Silicon/Intel)
- Unity Hub with Unity 6000.3.8f1 installed
- Visual Studio Code, Rider, or Visual Studio 2022+ for IDE support
- .NET 6.0+ SDK recommended for IDE tooling (though Unity bundles mono)

**Runtime:**
- macOS 10.13+ (minimum for macOS build target)
- At least 2GB RAM recommended
- GPU with basic OpenGL/Metal support

---

*Stack analysis: 2026-08-12*
