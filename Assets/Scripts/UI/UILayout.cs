using UnityEngine;

namespace ARGeometryGame.UI
{
    public static class UILayout
    {
        public static void SetFullScreen(RectTransform rt, int padding = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padding, padding);
            rt.offsetMax = new Vector2(-padding, -padding);
        }

        public static void SetTop(RectTransform rt, float height, int padding = 16)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(padding, -height - padding);
            rt.offsetMax = new Vector2(-padding, -padding);
        }

        public static void SetBottom(RectTransform rt, float height, int padding = 16)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.offsetMin = new Vector2(padding, padding);
            rt.offsetMax = new Vector2(-padding, height + padding);
        }
    }
}

