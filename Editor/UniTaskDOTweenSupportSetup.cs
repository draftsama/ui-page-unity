using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace UIPage.Editor
{
    /// <summary>
    /// ui-page-unity uses DOTween's UniTask awaiter extensions, which only compile when the
    /// UNITASK_DOTWEEN_SUPPORT define is set. This is a per-project Player Settings define,
    /// so it doesn't come along when this package is cloned/updated as a submodule.
    /// This checks for it on editor load and offers to add it automatically.
    /// </summary>
    [InitializeOnLoad]
    internal static class UniTaskDOTweenSupportSetup
    {
        private const string DefineSymbol = "UNITASK_DOTWEEN_SUPPORT";
        private const string SkippedSessionKey = "UIPage.UniTaskDOTweenSupportSetup.Skipped";

        private static readonly BuildTargetGroup[] TargetGroups =
        {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS,
            BuildTargetGroup.WebGL,
        };

        static UniTaskDOTweenSupportSetup()
        {
            EditorApplication.delayCall += CheckAndPrompt;
        }

        private static void CheckAndPrompt()
        {
            if (SessionState.GetBool(SkippedSessionKey, false))
                return;

            if (HasDefineSymbol(CurrentNamedBuildTarget()))
                return;

            if (!HasAssembly("DOTween") || !HasAssembly("UniTask"))
                return; // dependencies not installed yet, nothing to wire up

            var addIt = EditorUtility.DisplayDialog(
                "ui-page-unity",
                "ui-page-unity uses DOTween's UniTask integration, which requires the "
                + $"'{DefineSymbol}' scripting define symbol.\n\n"
                + "DOTween and UniTask were both detected in this project. Add the define symbol now?",
                "Add Symbol",
                "Skip for now");

            if (addIt)
                AddDefineSymbolToAllTargets();
            else
                SessionState.SetBool(SkippedSessionKey, true);
        }

        private static NamedBuildTarget CurrentNamedBuildTarget()
        {
            return NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        private static bool HasDefineSymbol(NamedBuildTarget target)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbols(target).Split(';');
            return defines.Contains(DefineSymbol);
        }

        private static void AddDefineSymbolToAllTargets()
        {
            foreach (var group in TargetGroups)
            {
                NamedBuildTarget namedTarget;
                try
                {
                    namedTarget = NamedBuildTarget.FromBuildTargetGroup(group);
                }
                catch
                {
                    continue; // group not supported in this editor/module installation
                }

                var defines = PlayerSettings.GetScriptingDefineSymbols(namedTarget)
                    .Split(';')
                    .Where(d => !string.IsNullOrEmpty(d))
                    .ToList();

                if (defines.Contains(DefineSymbol))
                    continue;

                defines.Add(DefineSymbol);
                PlayerSettings.SetScriptingDefineSymbols(namedTarget, string.Join(";", defines));
            }

            Debug.Log($"[ui-page-unity] Added '{DefineSymbol}' scripting define symbol.");
        }

        private static bool HasAssembly(string nameContains)
        {
            return System.AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.GetName().Name.IndexOf(nameContains, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
