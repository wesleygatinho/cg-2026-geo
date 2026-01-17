using ARGeometryGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ARGeometryGame.UI
{
    public sealed class MenuUIInstaller : MonoBehaviour
    {
        private void Start()
        {
            var canvas = UIFactory.EnsureCanvas("UI");
            var root = canvas.GetComponent<RectTransform>();

            var title = UIFactory.CreateText(root, "Title", "Geometria em AR", 60, TextAnchor.UpperCenter);
            UILayout.SetTop(title.rectTransform, 120);

            var info = UIFactory.CreateText(root, "Info", "Detecte um plano e responda as questões de geometria.", 34, TextAnchor.MiddleCenter);
            info.rectTransform.anchorMin = new Vector2(0.05f, 0.55f);
            info.rectTransform.anchorMax = new Vector2(0.95f, 0.75f);
            info.rectTransform.offsetMin = Vector2.zero;
            info.rectTransform.offsetMax = Vector2.zero;

            var flow = FindAnyObjectByType<MenuFlowController>();
            var startButton = UIFactory.CreateButton(root, "StartButton", "Iniciar", () => flow.StartGame());
            var startRt = startButton.GetComponent<RectTransform>();
            startRt.anchorMin = new Vector2(0.2f, 0.2f);
            startRt.anchorMax = new Vector2(0.8f, 0.35f);
            startRt.offsetMin = Vector2.zero;
            startRt.offsetMax = Vector2.zero;
        }
    }
}
