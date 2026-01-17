using UnityEngine;
using UnityEngine.EventSystems;

namespace ARGeometryGame.AR
{
    public static class UIEventSystemBootstrapper
    {
        public static void EnsureEventSystemExists()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }
}

