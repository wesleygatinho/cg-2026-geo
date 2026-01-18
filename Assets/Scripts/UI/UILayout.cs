using UnityEngine;

namespace ARGeometryGame.UI
{
    public static class UILayout
    {
        // Padding padrão para Safe Area em pixels
        private const float MIN_SAFE_PADDING = 80f;

        /// <summary>
        /// Retorna o padding necessário para Safe Area do dispositivo
        /// </summary>
        public static Vector4 GetSafeAreaPadding()
        {
            var safeArea = Screen.safeArea;
            var screenHeight = Screen.height;
            var screenWidth = Screen.width;

            // Calcula padding baseado na Safe Area
            float left = safeArea.x;
            float bottom = safeArea.y;
            float right = screenWidth - (safeArea.x + safeArea.width);
            float top = screenHeight - (safeArea.y + safeArea.height);

            // Garante um mínimo para navegação gestual do Android
            bottom = Mathf.Max(bottom, MIN_SAFE_PADDING);

            return new Vector4(left, bottom, right, top);
        }

        public static void SetFullScreen(RectTransform rt, int padding = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padding, padding);
            rt.offsetMax = new Vector2(-padding, -padding);
        }

        public static void SetTop(RectTransform rt, float height, int padding = 16)
        {
            var safeArea = GetSafeAreaPadding();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(padding + safeArea.x, -height - padding - safeArea.w);
            rt.offsetMax = new Vector2(-padding - safeArea.z, -padding - safeArea.w);
        }

        public static void SetBottom(RectTransform rt, float height, int padding = 16)
        {
            var safeArea = GetSafeAreaPadding();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.offsetMin = new Vector2(padding + safeArea.x, padding + safeArea.y);
            rt.offsetMax = new Vector2(-padding - safeArea.z, height + padding + safeArea.y);
        }

        /// <summary>
        /// Posiciona elemento na parte inferior respeitando Safe Area
        /// </summary>
        public static void SetBottomSafe(RectTransform rt, float anchorMinY, float anchorMaxY, float horizontalPadding = 0.05f)
        {
            var safeArea = GetSafeAreaPadding();
            float safeBottomPercent = safeArea.y / Screen.height;
            
            // Ajusta anchorMin.y para ficar acima da safe area
            float adjustedMinY = Mathf.Max(anchorMinY, safeBottomPercent + 0.02f);
            float adjustedMaxY = adjustedMinY + (anchorMaxY - anchorMinY);

            rt.anchorMin = new Vector2(horizontalPadding, adjustedMinY);
            rt.anchorMax = new Vector2(1f - horizontalPadding, adjustedMaxY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}



