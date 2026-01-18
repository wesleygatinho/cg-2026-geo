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

            // === Título divertido ===
            var title = UIFactory.CreateText(root, "Title", "🔷 Geometria em AR 🔷", 56, TextAnchor.UpperCenter);
            title.fontStyle = FontStyle.Bold;
            UILayout.SetTop(title.rectTransform, 140);

            // === Descrição informativa ===
            var info = UIFactory.CreateText(root, "Info", "📱 Aponte a câmera para o chão\n e descubra formas geométricas!\n\n🎯 Responda as questões corretamente!", 36, TextAnchor.MiddleCenter);
            info.rectTransform.anchorMin = new Vector2(0.05f, 0.45f);
            info.rectTransform.anchorMax = new Vector2(0.95f, 0.72f);
            info.rectTransform.offsetMin = Vector2.zero;
            info.rectTransform.offsetMax = Vector2.zero;

            // === Botão Iniciar com Safe Area ===
            var flow = FindAnyObjectByType<MenuFlowController>();
            var startButton = UIFactory.CreateButton(root, "StartButton", "🚀 Iniciar Jogo", () => flow.StartGame(), UIFactory.ColorSuccess);
            var startRt = startButton.GetComponent<RectTransform>();
            UILayout.SetBottomSafe(startRt, 0.15f, 0.30f, 0.12f);
        }
    }
}
