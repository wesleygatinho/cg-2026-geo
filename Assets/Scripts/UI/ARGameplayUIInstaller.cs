using ARGeometryGame.Gameplay;
using ARGeometryGame.Geometry;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace ARGeometryGame.UI
{
    public sealed class ARGameplayUIInstaller : MonoBehaviour
    {
        private Text _prompt;
        private Text _status;
        private InputField _answer;
        private Text _feedback;

        private ARGameplayQuizController _quiz;
        private ARSessionState _arState;
        private NotTrackingReason _notTrackingReason;

        private void Start()
        {
            _quiz = FindAnyObjectByType<ARGameplayQuizController>();
            var canvas = UIFactory.EnsureCanvas("UI");
            var root = canvas.GetComponent<RectTransform>();

            _prompt = UIFactory.CreateText(root, "Prompt", "Carregando...", 34, TextAnchor.UpperLeft);
            _prompt.rectTransform.anchorMin = new Vector2(0.05f, 0.72f);
            _prompt.rectTransform.anchorMax = new Vector2(0.95f, 0.95f);
            _prompt.rectTransform.offsetMin = Vector2.zero;
            _prompt.rectTransform.offsetMax = Vector2.zero;

            _status = UIFactory.CreateText(root, "Status", "", 28, TextAnchor.UpperLeft);
            _status.rectTransform.anchorMin = new Vector2(0.05f, 0.63f);
            _status.rectTransform.anchorMax = new Vector2(0.95f, 0.72f);
            _status.rectTransform.offsetMin = Vector2.zero;
            _status.rectTransform.offsetMax = Vector2.zero;

            _feedback = UIFactory.CreateText(root, "Feedback", "", 30, TextAnchor.MiddleCenter);
            _feedback.rectTransform.anchorMin = new Vector2(0.05f, 0.52f);
            _feedback.rectTransform.anchorMax = new Vector2(0.95f, 0.63f);
            _feedback.rectTransform.offsetMin = Vector2.zero;
            _feedback.rectTransform.offsetMax = Vector2.zero;

            _answer = UIFactory.CreateInputField(root, "Answer", "Digite sua resposta...");
            var answerRt = _answer.GetComponent<RectTransform>();
            answerRt.anchorMin = new Vector2(0.05f, 0.40f);
            answerRt.anchorMax = new Vector2(0.95f, 0.50f);
            answerRt.offsetMin = Vector2.zero;
            answerRt.offsetMax = Vector2.zero;

            var submit = UIFactory.CreateButton(root, "Submit", "Responder", () => _quiz.SubmitAnswer(_answer.text));
            var submitRt = submit.GetComponent<RectTransform>();
            submitRt.anchorMin = new Vector2(0.05f, 0.26f);
            submitRt.anchorMax = new Vector2(0.48f, 0.37f);
            submitRt.offsetMin = Vector2.zero;
            submitRt.offsetMax = Vector2.zero;

            var skip = UIFactory.CreateButton(root, "Skip", "Pular", () => _quiz.SkipQuestion());
            var skipRt = skip.GetComponent<RectTransform>();
            skipRt.anchorMin = new Vector2(0.52f, 0.26f);
            skipRt.anchorMax = new Vector2(0.95f, 0.37f);
            skipRt.offsetMin = Vector2.zero;
            skipRt.offsetMax = Vector2.zero;

            var planes = UIFactory.CreateButton(root, "Planes", "Mostrar/Esconder Planos", () => _quiz.TogglePlanes());
            var planesRt = planes.GetComponent<RectTransform>();
            planesRt.anchorMin = new Vector2(0.05f, 0.12f);
            planesRt.anchorMax = new Vector2(0.95f, 0.23f);
            planesRt.offsetMin = Vector2.zero;
            planesRt.offsetMax = Vector2.zero;

            HookQuiz();
        }

        private void Update()
        {
            _arState = ARSession.state;
            _notTrackingReason = ARSession.notTrackingReason;
            _status.text = BuildStatusText(Mathf.Max(_quiz != null ? _quiz.CurrentQuestionIndex + 1 : 0, 0), Mathf.Max(_quiz != null ? _quiz.TotalQuestions : 0, 0));
        }

        private void HookQuiz()
        {
            if (_quiz == null)
            {
                _prompt.text = "Erro: controlador de quiz não encontrado.";
                return;
            }

            _quiz.QuestionChanged += HandleQuestionChanged;
            _quiz.FeedbackChanged += HandleFeedback;

            HandleQuestionChanged(_quiz.CurrentQuestion, Mathf.Max(_quiz.CurrentQuestionIndex + 1, 0), Mathf.Max(_quiz.TotalQuestions, 0));
        }

        private void OnDestroy()
        {
            if (_quiz == null)
            {
                return;
            }

            _quiz.QuestionChanged -= HandleQuestionChanged;
            _quiz.FeedbackChanged -= HandleFeedback;
        }

        private void HandleQuestionChanged(GeometryQuestion q, int index, int total)
        {
            _prompt.text = q?.prompt ?? "Sem questão.";
            _status.text = BuildStatusText(index, total);
            _answer.text = "";
            _feedback.text = "";
        }

        private void HandleFeedback(string msg)
        {
            _feedback.text = msg ?? "";
        }

        private string BuildStatusText(int index, int total)
        {
            var ar = _arState != ARSessionState.None ? $"AR: {_arState}" : "AR: (inicializando)";
            var reason = _arState == ARSessionState.SessionTracking ? "" : $" ({_notTrackingReason})";

            var camPerm = GetCameraPermissionLabel();
            var xrLabel = GetXRLoaderLabel();
            var hint = GetPlacementHint();

            return $"Questão {index}/{total}. {hint}\n{ar}{reason}\n{xrLabel}\n{camPerm}";
        }

        private string GetPlacementHint()
        {
            var loaderActive = XRGeneralSettings.Instance != null &&
                               XRGeneralSettings.Instance.Manager != null &&
                               XRGeneralSettings.Instance.Manager.activeLoader != null;

            if (!loaderActive || _arState == ARSessionState.Unsupported)
            {
                return "Toque na tela para colocar o objeto (modo câmera).";
            }

            return "Toque no plano para colocar o objeto.";
        }

        private static string GetCameraPermissionLabel()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return "Câmera: ok (não-Android)";
            }

            var granted = UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera);
            return granted ? "Câmera: permissão OK" : "Câmera: sem permissão (verifique prompt / Configurações)";
        }

        private static string GetXRLoaderLabel()
        {
            var settings = XRGeneralSettings.Instance;
            if (settings == null)
            {
                return "XR: XRGeneralSettings = null";
            }

            if (settings.Manager == null)
            {
                return "XR: Manager = null";
            }

            var loader = settings.Manager.activeLoader;
            return loader != null ? $"XR: loader ativo = {loader.name}" : "XR: loader ativo = null";
        }
    }
}
