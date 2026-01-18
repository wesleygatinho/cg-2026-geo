using ARGeometryGame.Core;
using UnityEngine;

using UnityEngine.UI;

namespace ARGeometryGame.UI
{
    public sealed class ResultsUIInstaller : MonoBehaviour
    {
        private void Start()
        {
            var canvas = UIFactory.EnsureCanvas("UI");
            var root = canvas.GetComponent<RectTransform>();

            // === Título festivo ===
            var title = UIFactory.CreateText(root, "Title", "🎉 Resultados 🎉", 56, UnityEngine.TextAnchor.UpperCenter);
            title.fontStyle = FontStyle.Bold;
            UILayout.SetTop(title.rectTransform, 140);

            // === Resumo colorido ===
            var summary = UIFactory.CreateText(root, "Summary", BuildSummary(), 38, UnityEngine.TextAnchor.MiddleCenter);
            summary.rectTransform.anchorMin = new Vector2(0.05f, 0.40f);
            summary.rectTransform.anchorMax = new Vector2(0.95f, 0.72f);
            summary.rectTransform.offsetMin = Vector2.zero;
            summary.rectTransform.offsetMax = Vector2.zero;

            // === Botão Voltar com Safe Area ===
            var flow = FindAnyObjectByType<ResultsFlowController>();
            var back = UIFactory.CreateButton(root, "BackButton", "🏠 Voltar ao Menu", () => flow.BackToMenu(), UIFactory.ColorPrimary);
            var backRt = back.GetComponent<RectTransform>();
            UILayout.SetBottomSafe(backRt, 0.15f, 0.30f, 0.12f);
        }

        private static string BuildSummary()
        {
            var best = PlayerProgress.GetBestScore();
            var correct = GameSession.CorrectAnswers;
            var total = GameSession.TotalQuestions;
            
            // Emoji baseado no desempenho
            string emoji = correct >= total * 0.8f ? "🏆" : correct >= total * 0.5f ? "👍" : "💪";
            
            return $"{emoji} Acertos: {correct}/{total}\n" +
                   $"📝 Tentativas: {GameSession.Attempts}\n" +
                   $"⏱️ Tempo: {GameSession.ElapsedSeconds:0.0}s\n" +
                   $"⭐ Score: {GameSession.Score}\n" +
                   $"🏅 Recorde: {best}";
        }
    }
}
