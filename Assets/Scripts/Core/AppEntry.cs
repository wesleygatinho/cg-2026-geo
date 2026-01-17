using UnityEngine;

namespace ARGeometryGame.Core
{
    public static class AppEntry
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var existing = Object.FindAnyObjectByType<AppRoot>();
            if (existing != null)
            {
                return;
            }

            var go = new GameObject("App");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<AppRoot>();
        }
    }
}

