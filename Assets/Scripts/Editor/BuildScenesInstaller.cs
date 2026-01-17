using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace ARGeometryGame.Editor
{
    [InitializeOnLoad]
    public static class BuildScenesInstaller
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/Scenes/Boot.unity",
            "Assets/Scenes/Menu.unity",
            "Assets/Scenes/ARGameplay.unity",
            "Assets/Scenes/Results.unity"
        };

        static BuildScenesInstaller()
        {
            EnsureScenesInBuildSettings();
        }

        [MenuItem("Tools/AR Geometry Game/Instalar Cenas no Build")]
        public static void EnsureScenesInBuildSettings()
        {
            var existing = EditorBuildSettings.scenes?.ToList() ?? new List<EditorBuildSettingsScene>();
            var existingPaths = new HashSet<string>(existing.Select(s => s.path));

            var changed = false;
            foreach (var path in ScenePaths)
            {
                if (existingPaths.Contains(path))
                {
                    continue;
                }

                existing.Add(new EditorBuildSettingsScene(path, true));
                changed = true;
            }

            if (changed)
            {
                EditorBuildSettings.scenes = existing.ToArray();
            }
        }
    }
}

