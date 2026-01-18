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
        private Button _togglePlanesButton;
        private Text _togglePlanesLabel;

        private ARGameplayQuizController _quiz;
        private ARSessionState _arState;
        private NotTrackingReason _notTrackingReason;

        private void Start()
        {
            _quiz = FindAnyObjectByType<ARGameplayQuizController>();
            var canvas = UIFactory.EnsureCanvas("UI");
            var root = canvas.GetComponent<RectTransform>();

            // === ÁREA SUPERIOR: Prompt da questão ===
            _prompt = UIFactory.CreateText(root, "Prompt", "🎮 Carregando...", 40, TextAnchor.UpperLeft);
            _prompt.fontStyle = FontStyle.Bold;
            _prompt.rectTransform.anchorMin = new Vector2(0.03f, 0.75f);
            _prompt.rectTransform.anchorMax = new Vector2(0.97f, 0.96f);
            _prompt.rectTransform.offsetMin = Vector2.zero;
            _prompt.rectTransform.offsetMax = Vector2.zero;

            // === Status AR (menor, para debug) ===
            _status = UIFactory.CreateText(root, "Status", "", 22, TextAnchor.UpperLeft);
            _status.color = new Color(1f, 1f, 1f, 0.7f);
            _status.rectTransform.anchorMin = new Vector2(0.03f, 0.67f);
            _status.rectTransform.anchorMax = new Vector2(0.97f, 0.75f);
            _status.rectTransform.offsetMin = Vector2.zero;
            _status.rectTransform.offsetMax = Vector2.zero;

            // === Feedback (correto/incorreto) ===
            _feedback = UIFactory.CreateText(root, "Feedback", "", 36, TextAnchor.MiddleCenter);
            _feedback.fontStyle = FontStyle.Bold;
            _feedback.rectTransform.anchorMin = new Vector2(0.03f, 0.55f);
            _feedback.rectTransform.anchorMax = new Vector2(0.97f, 0.67f);
            _feedback.rectTransform.offsetMin = Vector2.zero;
            _feedback.rectTransform.offsetMax = Vector2.zero;

            // === Campo de resposta - COMPACTO ===
            _answer = UIFactory.CreateInputField(root, "Answer", "✏️ Digite sua resposta...");
            var answerRt = _answer.GetComponent<RectTransform>();
            answerRt.anchorMin = new Vector2(0.05f, 0.18f);
            answerRt.anchorMax = new Vector2(0.95f, 0.25f);
            answerRt.offsetMin = Vector2.zero;
            answerRt.offsetMax = Vector2.zero;

            // === BOTÕES COMPACTOS E ELEGANTES ===
            // Botão Responder - Verde (menor e mais embaixo)
            var submit = UIFactory.CreateButton(root, "Submit", "✅ Responder", () => _quiz.SubmitAnswer(_answer.text), UIFactory.ColorSuccess);
            var submitRt = submit.GetComponent<RectTransform>();
            submitRt.anchorMin = new Vector2(0.05f, 0.10f);
            submitRt.anchorMax = new Vector2(0.32f, 0.17f);
            submitRt.offsetMin = Vector2.zero;
            submitRt.offsetMax = Vector2.zero;

            // Botão Pular - Laranja (menor)
            var skip = UIFactory.CreateButton(root, "Skip", "⏭️ Pular", () => _quiz.SkipQuestion(), UIFactory.ColorWarning);
            var skipRt = skip.GetComponent<RectTransform>();
            skipRt.anchorMin = new Vector2(0.34f, 0.10f);
            skipRt.anchorMax = new Vector2(0.61f, 0.17f);
            skipRt.offsetMin = Vector2.zero;
            skipRt.offsetMax = Vector2.zero;

            // Botão Mostrar/Esconder Planos - Azul (menor)
            _togglePlanesButton = UIFactory.CreateButton(root, "Planes", "👁️ Planos", OnTogglePlanes, UIFactory.ColorPrimary);
            var planesRt = _togglePlanesButton.GetComponent<RectTransform>();
            planesRt.anchorMin = new Vector2(0.63f, 0.10f);
            planesRt.anchorMax = new Vector2(0.95f, 0.17f);
            planesRt.offsetMin = Vector2.zero;
            planesRt.offsetMax = Vector2.zero;
            _togglePlanesLabel = _togglePlanesButton.GetComponentInChildren<Text>();

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
            
            // Colorir feedback baseado no conteúdo
            if (msg != null && msg.Contains("Correto"))
            {
                _feedback.color = UIFactory.ColorSuccess;
            }
            else if (msg != null && msg.Contains("Incorreto"))
            {
                _feedback.color = UIFactory.ColorDanger;
            }
            else
            {
                _feedback.color = Color.white;
            }
        }

        private bool _planesVisible = true;

        private void OnTogglePlanes()
        {
            if (_quiz == null) return;
            
            _planesVisible = !_planesVisible;
            _quiz.TogglePlanes();
            
            // Atualiza texto do botão
            if (_togglePlanesLabel != null)
            {
                _togglePlanesLabel.text = _planesVisible ? "🙈 Esconder Planos" : "👁️ Mostrar Planos";
            }
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
