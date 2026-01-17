using ARGeometryGame.Core;
using UnityEngine;

namespace ARGeometryGame.UI
{
    public sealed class ResultsUIInstaller : MonoBehaviour
    {
        private void Start()
        {
            var canvas = UIFactory.EnsureCanvas("UI");
            var root = canvas.GetComponent<RectTransform>();

            var title = UIFactory.CreateText(root, "Title", "Resultados", 60, UnityEngine.TextAnchor.UpperCenter);
            UILayout.SetTop(title.rectTransform, 120);

            var summary = UIFactory.CreateText(root, "Summary", BuildSummary(), 36, UnityEngine.TextAnchor.MiddleCenter);
            summary.rectTransform.anchorMin = new Vector2(0.05f, 0.45f);
            summary.rectTransform.anchorMax = new Vector2(0.95f, 0.70f);
            summary.rectTransform.offsetMin = Vector2.zero;
            summary.rectTransform.offsetMax = Vector2.zero;

            var flow = FindAnyObjectByType<ResultsFlowController>();
            var back = UIFactory.CreateButton(root, "BackButton", "Voltar ao Menu", () => flow.BackToMenu());
            var backRt = back.GetComponent<RectTransform>();
            backRt.anchorMin = new Vector2(0.2f, 0.2f);
            backRt.anchorMax = new Vector2(0.8f, 0.35f);
            backRt.offsetMin = Vector2.zero;
            backRt.offsetMax = Vector2.zero;
        }

        private static string BuildSummary()
        {
            var best = PlayerProgress.GetBestScore();
            return $"Acertos: {GameSession.CorrectAnswers}/{GameSession.TotalQuestions}\nTentativas: {GameSession.Attempts}\nTempo: {GameSession.ElapsedSeconds:0.0}s\nScore: {GameSession.Score}\nRecorde: {best}";
        }
    }
}
