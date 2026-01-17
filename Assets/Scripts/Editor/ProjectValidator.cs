using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ARGeometryGame.Editor
{
    public sealed class ProjectValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var issues = Validate();
            if (issues.Count == 0)
            {
                return;
            }

            foreach (var issue in issues)
            {
                Debug.LogWarning(issue);
            }
        }

        [MenuItem("Tools/AR Geometry Game/Validar Projeto")]
        public static void ValidateMenu()
        {
            var issues = Validate();
            if (issues.Count == 0)
            {
                Debug.Log("Validação OK: configurações básicas parecem corretas.");
                return;
            }

            Debug.LogWarning($"Validação encontrou {issues.Count} item(ns):");
            foreach (var issue in issues)
            {
                Debug.LogWarning(issue);
            }
        }

        private static List<string> Validate()
        {
            var issues = new List<string>();

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                issues.Add("BuildTarget não é Android. Ajuste em File > Build Settings.");
                return issues;
            }

            var scriptingBackend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android);
            if (scriptingBackend != ScriptingImplementation.IL2CPP)
            {
                issues.Add("Scripting Backend não é IL2CPP (recomendado para ARM64/Play Store).");
            }

            var arch = PlayerSettings.Android.targetArchitectures;
            if ((arch & AndroidArchitecture.ARM64) == 0)
            {
                issues.Add("Arquitetura ARM64 não está habilitada (Player Settings > Other Settings).");
            }

            var minSdk = PlayerSettings.Android.minSdkVersion;
            if (minSdk < AndroidSdkVersions.AndroidApiLevel24)
            {
                issues.Add("Min SDK está abaixo de 24. ARCore geralmente requer API 24+.");
            }

            return issues;
        }
    }
}

