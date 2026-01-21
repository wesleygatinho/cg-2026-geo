using ARGeometryGame.Gameplay;
using ARGeometryGame.Geometry;
using UnityEngine;
using UnityEngine.UI;

namespace ARGeometryGame.UI
{
    /// <summary>
    /// Cria e gerencia a UI do gameplay com visual moderno usando apenas componentes nativos.
    /// Não requer sprites customizados - funciona garantido.
    /// </summary>
    public sealed class ARGameplayUIInstaller : MonoBehaviour
    {
        [Header("Configurações")]
        [SerializeField] private int fontSize = 32;
        [SerializeField] private float panelAlpha = 0.85f;

        // Cores modernas
        private readonly Color _panelColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        private readonly Color _inputBgColor = new Color(1f, 1f, 1f, 0.95f);
        private readonly Color _greenBtn = new Color(0.18f, 0.8f, 0.44f, 1f);
        private readonly Color _orangeBtn = new Color(1f, 0.6f, 0.2f, 1f);
        private readonly Color _blueBtn = new Color(0.2f, 0.6f, 0.95f, 1f);
        private readonly Color _successColor = new Color(0.18f, 0.8f, 0.44f, 0.95f);
        private readonly Color _errorColor = new Color(0.9f, 0.3f, 0.25f, 0.95f);

        private Text _promptText;
        private Text _progressText;
        private InputField _answerInput;
        private GameObject _feedbackPanel;
        private Text _feedbackText;
        private Image _feedbackImage;

        private ARGameplayQuizController _quiz;
        private float _feedbackHideTime;
        private bool _planesVisible = true;

        private void Start()
        {
            _quiz = FindAnyObjectByType<ARGameplayQuizController>();
            
            // Destruir Canvas antigo se existir
            var oldCanvas = FindAnyObjectByType<Canvas>();
            if (oldCanvas != null && oldCanvas.name == "GameplayCanvas")
            {
                Destroy(oldCanvas.gameObject);
            }

            CreateUI();
            HookQuiz();
        }

        private void Update()
        {
            if (_feedbackPanel != null && _feedbackPanel.activeSelf && Time.time > _feedbackHideTime)
            {
                _feedbackPanel.SetActive(false);
            }
        }

        private void CreateUI()
        {
            // === CANVAS ===
            var canvasGo = new GameObject("GameplayUI");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();

            var root = canvas.GetComponent<RectTransform>();

            // === PAINEL SUPERIOR (Pergunta) ===
            var topPanel = CreatePanel(root, "TopPanel", _panelColor);
            SetAnchors(topPanel.rectTransform, 0.02f, 0.88f, 0.98f, 0.98f);

            _promptText = CreateText(topPanel.transform, "PromptText", "Carregando...", fontSize, FontStyle.Bold);
            SetAnchors(_promptText.rectTransform, 0.03f, 0.1f, 0.78f, 0.9f);
            _promptText.alignment = TextAnchor.MiddleLeft;

            // Badge de progresso
            var badge = CreatePanel(topPanel.transform, "Badge", _blueBtn);
            SetAnchors(badge.rectTransform, 0.80f, 0.2f, 0.97f, 0.8f);

            _progressText = CreateText(badge.transform, "ProgressText", "1/10", 28, FontStyle.Bold);
            SetAnchors(_progressText.rectTransform, 0, 0, 1, 1);
            _progressText.alignment = TextAnchor.MiddleCenter;

            // === PAINEL INFERIOR (Controles) ===
            var bottomPanel = CreatePanel(root, "BottomPanel", _panelColor);
            SetAnchors(bottomPanel.rectTransform, 0.02f, 0.02f, 0.98f, 0.18f);

            // Input Field
            var inputBg = CreatePanel(bottomPanel.transform, "InputBg", _inputBgColor);
            SetAnchors(inputBg.rectTransform, 0.03f, 0.55f, 0.97f, 0.92f);

            _answerInput = CreateInputField(inputBg, "AnswerInput");

            // Botões
            var btnRow = new GameObject("ButtonRow");
            btnRow.transform.SetParent(bottomPanel.transform, false);
            var btnRowRt = btnRow.AddComponent<RectTransform>();
            SetAnchors(btnRowRt, 0.03f, 0.08f, 0.97f, 0.50f);

            var layout = btnRow.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 15;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            CreateButton(btnRow.transform, "Submit", "Responder", _greenBtn, OnSubmit);
            CreateButton(btnRow.transform, "Skip", "Pular", _orangeBtn, OnSkip);
            // Botão de planos desabilitado - malha não está sendo usada
            // CreateButton(btnRow.transform, "Planes", "Planos", _blueBtn, OnTogglePlanes);

            // === PAINEL DE FEEDBACK (Centro) ===
            _feedbackPanel = CreatePanel(root, "FeedbackPanel", _successColor).gameObject;
            var feedbackRt = _feedbackPanel.GetComponent<RectTransform>();
            SetAnchors(feedbackRt, 0.1f, 0.45f, 0.9f, 0.55f);
            _feedbackImage = _feedbackPanel.GetComponent<Image>();

            _feedbackText = CreateText(_feedbackPanel.transform, "FeedbackText", "", 30, FontStyle.Bold);
            SetAnchors(_feedbackText.rectTransform, 0.05f, 0.1f, 0.95f, 0.9f);
            _feedbackText.alignment = TextAnchor.MiddleCenter;

            _feedbackPanel.SetActive(false);
        }

        private Image CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            go.AddComponent<RectTransform>();
            return img;
        }

        private Text CreateText(Transform parent, string name, string content, int size, FontStyle style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = size;
            txt.fontStyle = style;
            txt.color = Color.white;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return txt;
        }

        private InputField CreateInputField(Image bg, string name)
        {
            var input = bg.gameObject.AddComponent<InputField>();

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(bg.transform, false);
            var txt = textGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 28;
            txt.color = Color.black;
            txt.alignment = TextAnchor.MiddleLeft;
            SetAnchors(txt.rectTransform, 0.02f, 0, 0.98f, 1);

            var phGo = new GameObject("Placeholder");
            phGo.transform.SetParent(bg.transform, false);
            var ph = phGo.AddComponent<Text>();
            ph.font = txt.font;
            ph.fontSize = 28;
            ph.fontStyle = FontStyle.Italic;
            ph.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
            ph.text = "Digite sua resposta...";
            ph.alignment = TextAnchor.MiddleLeft;
            SetAnchors(ph.rectTransform, 0.02f, 0, 0.98f, 1);

            input.textComponent = txt;
            input.placeholder = ph;
            input.contentType = InputField.ContentType.DecimalNumber;

            return input;
        }

        private void CreateButton(Transform parent, string name, string label, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = color;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            // Efeito de press
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            btn.colors = colors;

            var txt = CreateText(go.transform, "Label", label, 26, FontStyle.Bold);
            SetAnchors(txt.rectTransform, 0, 0, 1, 1);
            txt.alignment = TextAnchor.MiddleCenter;

            go.AddComponent<LayoutElement>();
        }

        private void SetAnchors(RectTransform rt, float minX, float minY, float maxX, float maxY)
        {
            rt.anchorMin = new Vector2(minX, minY);
            rt.anchorMax = new Vector2(maxX, maxY);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;

            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private void HookQuiz()
        {
            if (_quiz == null)
            {
                _promptText.text = "Aguardando quiz...";
                return;
            }

            _quiz.QuestionChanged += OnQuestionChanged;
            _quiz.FeedbackChanged += OnFeedback;

            OnQuestionChanged(_quiz.CurrentQuestion, 
                Mathf.Max(_quiz.CurrentQuestionIndex + 1, 1), 
                Mathf.Max(_quiz.TotalQuestions, 1));
        }

        private void OnDestroy()
        {
            if (_quiz != null)
            {
                _quiz.QuestionChanged -= OnQuestionChanged;
                _quiz.FeedbackChanged -= OnFeedback;
            }
        }

        private void OnQuestionChanged(GeometryQuestion q, int idx, int total)
        {
            if (_promptText != null) _promptText.text = q?.prompt ?? "Carregando...";
            if (_progressText != null) _progressText.text = $"{idx}/{total}";
            if (_answerInput != null) _answerInput.text = "";
            if (_feedbackPanel != null) _feedbackPanel.SetActive(false);
        }

        private void OnFeedback(string msg)
        {
            if (string.IsNullOrEmpty(msg) || _feedbackPanel == null) return;

            _feedbackText.text = msg;
            _feedbackPanel.SetActive(true);
            _feedbackHideTime = Time.time + 3f;

            if (_feedbackImage != null)
            {
                _feedbackImage.color = msg.Contains("Correto") ? _successColor : _errorColor;
            }
        }

        private void OnSubmit()
        {
            if (_quiz != null && _answerInput != null)
                _quiz.SubmitAnswer(_answerInput.text);
        }

        private void OnSkip() => _quiz?.SkipQuestion();

        private void OnTogglePlanes()
        {
            _planesVisible = !_planesVisible;
            _quiz?.TogglePlanes();
        }
    }
}
