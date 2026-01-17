using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ARGeometryGame.UI
{
    public static class UIFactory
    {
        public static Canvas EnsureCanvas(string name)
        {
            var existing = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (existing != null)
            {
                EnsureEventSystem();
                return existing;
            }

            var canvasGo = new GameObject(name);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();
            return canvas;
        }

        public static Text CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var uiText = go.AddComponent<Text>();
            uiText.text = text;
            uiText.fontSize = fontSize;
            uiText.alignment = alignment;
            uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
            uiText.verticalOverflow = VerticalWrapMode.Overflow;
            uiText.color = Color.white;
            uiText.font = GetFontSafe();
            return uiText;
        }

        public static Button CreateButton(Transform parent, string name, string label, UnityAction onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

            var button = go.AddComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            var text = CreateText(go.transform, "Label", label, 34, TextAnchor.MiddleCenter);
            var rt = text.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            return button;
        }

        public static InputField CreateInputField(Transform parent, string name, string placeholderText)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.9f);

            var input = go.AddComponent<InputField>();

            var text = CreateText(go.transform, "Text", "", 32, TextAnchor.MiddleLeft);
            text.color = Color.black;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var placeholder = CreateText(go.transform, "Placeholder", placeholderText, 32, TextAnchor.MiddleLeft);
            placeholder.color = new Color(0f, 0f, 0f, 0.45f);

            var textRt = text.rectTransform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(16, 8);
            textRt.offsetMax = new Vector2(-16, -8);

            var placeholderRt = placeholder.rectTransform;
            placeholderRt.anchorMin = Vector2.zero;
            placeholderRt.anchorMax = Vector2.one;
            placeholderRt.offsetMin = new Vector2(16, 8);
            placeholderRt.offsetMax = new Vector2(-16, -8);

            input.textComponent = text;
            input.placeholder = placeholder;
            input.contentType = InputField.ContentType.DecimalNumber;
            input.lineType = InputField.LineType.SingleLine;

            return input;
        }

        private static Font GetFontSafe()
        {
            Font font = null;
            try
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch
            {
            }

            if (font == null)
            {
                try
                {
                    font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                catch
                {
                }
            }

            return font;
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();

            var inputSystemType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemType != null)
            {
                go.AddComponent(inputSystemType);
                return;
            }

            go.AddComponent<StandaloneInputModule>();
        }
    }
}
