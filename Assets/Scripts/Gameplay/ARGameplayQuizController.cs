using System;
using System.Collections.Generic;
using ARGeometryGame.AR;
using ARGeometryGame.Core;
using ARGeometryGame.Geometry;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace ARGeometryGame.Gameplay
{
    public sealed class ARGameplayQuizController : MonoBehaviour
    {
        [SerializeField] private string questionFileName = "questions.json";
        [SerializeField] private int questionsPerRun = 10;

        [SerializeField] private ARPlacementManager placementManager;
        [SerializeField] private ARAnchorPlacementManager anchorPlacementManager;
        [SerializeField] private ARPlaneVisibilityController planeVisibilityController;
        [SerializeField] private ARRaycastManager raycastManager;

        private ARPlaneManager _planeManager;
        private readonly List<GeometryQuestion> _questions = new();
        private int _index;
        private Transform _anchorTransform;
        private GameObject _currentVisual;
        private string _lastFeedback;

        public GeometryQuestion CurrentQuestion => _index >= 0 && _index < _questions.Count ? _questions[_index] : null;
        public int CurrentQuestionIndex => _index;
        public int TotalQuestions => _questions.Count;
        public bool HasPlacedObject => _anchorTransform != null;

        public event Action<GeometryQuestion, int, int> QuestionChanged;
        public event Action<string> FeedbackChanged;

        private void Start()
        {
            EnsureReferences();
            LoadRunQuestions();
            RaiseQuestionChanged();
            if (!HasPlacedObject && ShouldUseFallbackPlacement())
            {
                TryPlaceFallback();
            }
            // If not placed, try to place on tap
            if (!HasPlacedObject)
            {
                if (!placementManager.TryGetPlacementPose(out var pose))
                {
                    return;
                }

                // In Fallback mode, we just use the pose from placement manager which simulates floor
                // But we need to check if user TAPPED
                
                // ... wait, the code below checks for touch inside TryHandlePlacementTap.
                // But TryHandlePlacementTap was previously calling ShouldUseFallbackPlacement.
                // Let's refactor to use the Reticle + Tap flow.
            }
        }

        private void Update()
        {
            if (!HasPlacedObject)
            {
                // Always update hint if we are scanning
                UpdatePlacementHint();
                TryHandlePlacementTap();
            }
        }

        private void UpdatePlacementHint()
        {
            if (HasPlacedObject) return;

            string msg = "Aponte para o chão e toque para colocar o objeto.";
            
            // If AR is working and we have no planes
            if (_planeManager != null && _planeManager.trackables.count == 0 && IsARReady())
            {
                msg = "Mova o celular para detectar o chão...";
            }

            if (_lastFeedback != msg)
            {
                _lastFeedback = msg;
                FeedbackChanged?.Invoke(msg);
            }
        }

        private bool _planesVisible = true;

        public void TogglePlanes()
        {
            if (planeVisibilityController == null)
            {
                return;
            }

            // Toggle usando estado interno em vez de verificar planos
            _planesVisible = !_planesVisible;
            planeVisibilityController.SetPlanesVisible(_planesVisible);
        }

        public void SubmitAnswer(string raw)
        {
            var q = CurrentQuestion;
            if (q == null)
            {
                return;
            }

            if (!GeometryAnswerValidator.TryParseAnswer(raw, out var parsed))
            {
                FeedbackChanged?.Invoke("Resposta inválida. Use número (ex: 0.25 ou 0,25).");
                return;
            }

            var correct = GeometryAnswerValidator.IsCorrect(q, parsed);
            GameSession.RegisterAttempt(correct);

            if (!correct)
            {
                var expected = GeometryAnswerValidator.ExpectedAnswer(q);
                var formula = GeometryFormulaHelper.GetFormula(q.shape, q.metric);
                FeedbackChanged?.Invoke($"Incorreto. {formula}\n(Dica: resultado ~ {expected:0.###} {q.unit})");
                return;
            }

            FeedbackChanged?.Invoke("Correto!");
            NextQuestionOrFinish();
        }

        public void SkipQuestion()
        {
            NextQuestionOrFinish();
        }

        private void NextQuestionOrFinish()
        {
            _index++;
            if (_index >= _questions.Count)
            {
                FindAnyObjectByType<ARGameplayFlowController>()?.FinishRun();
                return;
            }

            UpdatePlacedVisual();
            RaiseQuestionChanged();
        }

        private void RaiseQuestionChanged()
        {
            QuestionChanged?.Invoke(CurrentQuestion, _index + 1, _questions.Count);
        }

        private void EnsureReferences()
        {
            if (placementManager == null)
            {
                placementManager = FindAnyObjectByType<ARPlacementManager>();
                if (placementManager == null)
                {
                    placementManager = new GameObject("AR Placement").AddComponent<ARPlacementManager>();
                }
            }

            if (anchorPlacementManager == null)
            {
                anchorPlacementManager = FindAnyObjectByType<ARAnchorPlacementManager>();
                if (anchorPlacementManager == null)
                {
                    anchorPlacementManager = new GameObject("AR Anchors").AddComponent<ARAnchorPlacementManager>();
                }
            }

            if (planeVisibilityController == null)
            {
                planeVisibilityController = FindAnyObjectByType<ARPlaneVisibilityController>();
                if (planeVisibilityController == null)
                {
                    planeVisibilityController = new GameObject("AR Planes").AddComponent<ARPlaneVisibilityController>();
                }
            }

            if (raycastManager == null)
            {
                raycastManager = FindAnyObjectByType<ARRaycastManager>();
            }

            if (_planeManager == null)
            {
                _planeManager = FindAnyObjectByType<ARPlaneManager>();
            }

            var arSessionState = FindAnyObjectByType<ARSessionStateReporter>();
            if (arSessionState == null)
            {
                new GameObject("AR Session State").AddComponent<ARSessionStateReporter>();
            }
        }

        private void LoadRunQuestions()
        {
            var all = QuestionBankLoader.LoadQuestionsFromStreamingAssets(questionFileName);
            var seed = Environment.TickCount;
            var selected = QuestionSelector.SelectRandom(all, questionsPerRun, seed);
            _questions.Clear();
            _questions.AddRange(selected);
            _index = 0;
            GameSession.StartNew(_questions.Count);
        }

        private void TryHandlePlacementTap()
        {
            if (Input.touchCount < 1)
            {
                return;
            }

            var touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Began)
            {
                return;
            }

            // Check if pointer is over UI
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            // Usar posição do toque para raycast (não centro da tela)
            var touchPosition = touch.position;
            
            // Tentar raycast na posição do toque primeiro (AR mode)
            if (placementManager.TryGetPlacementHitAtPosition(touchPosition, out var hit))
            {
                // AR funcionando - usar o hit com pose e trackableId corretos
                var pose = hit.pose;
                var trackableId = hit.trackableId;
                
                // Criar âncora ANEXADA ao plano detectado para ficar fixa no mundo real
                var anchor = anchorPlacementManager.PlaceAnchor(pose, trackableId);
                if (anchor != null)
                {
                    _anchorTransform = anchor.transform;
                    planeVisibilityController.SetPlanesVisible(false);
                    UpdatePlacedVisual();
                    FeedbackChanged?.Invoke("✅ Objeto fixo no plano! Responda a questão.");
                    return;
                }
            }
            
            // Fallback: usar pose do centro da tela se raycast de toque falhar
            if (placementManager.TryGetPlacementPose(out var fallbackPose))
            {
                var go = new GameObject("Fallback Anchor");
                go.transform.position = fallbackPose.position;
                go.transform.rotation = fallbackPose.rotation;
                _anchorTransform = go.transform;
                
                planeVisibilityController.SetPlanesVisible(false);
                UpdatePlacedVisual();
                FeedbackChanged?.Invoke("📍 Objeto colocado (modo câmera). Responda a questão.");
            }
        }

        private bool ShouldUseFallbackPlacement()
        {
            if (!IsARReady())
            {
                return true;
            }

            var settings = XRGeneralSettings.Instance;
            if (settings == null || settings.Manager == null || settings.Manager.activeLoader == null)
            {
                return true;
            }

            return placementManager == null || anchorPlacementManager == null || raycastManager == null;
        }

        private static bool IsARReady()
        {
            var state = ARSession.state;
            if (state == ARSessionState.Unsupported)
            {
                return false;
            }

            return state == ARSessionState.SessionInitializing || state == ARSessionState.SessionTracking;
        }

        private void TryPlaceFallback()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            if (_anchorTransform == null)
            {
                var go = new GameObject("Fallback Anchor");
                _anchorTransform = go.transform;
            }

            var forward = cam.transform.forward;
            var flatForward = new Vector3(forward.x, 0f, forward.z);
            if (flatForward.sqrMagnitude < 0.001f)
            {
                flatForward = Vector3.forward;
            }

            // ONLY set position if not already placed
            if (_anchorTransform.parent == null && _anchorTransform.gameObject.scene.name != null) 
            {
                 // It's a scene object, maybe we shouldn't move it if it's already there?
                 // Actually, if we are in fallback mode, we want to place it ONCE in front of the user
                 // and then let the user move around it.
            }
            
            // In fallback mode, we just want to spawn it at a fixed distance once.
            // If it already exists, we do NOT move it to the camera again.
            if (_currentVisual != null)
            {
                 FeedbackChanged?.Invoke("Objeto já colocado. Mova-se para vê-lo.");
                 return;
            }

            _anchorTransform.position = cam.transform.position + forward.normalized * 1.5f;
            _anchorTransform.rotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up);

            UpdatePlacedVisual();
            FeedbackChanged?.Invoke("Objeto colocado (modo câmera). Responda a questão.");
        }

        private void UpdatePlacedVisual()
        {
            if (_anchorTransform == null)
            {
                return;
            }

            if (_currentVisual != null)
            {
                Destroy(_currentVisual);
            }

            var q = CurrentQuestion;
            if (q == null)
            {
                return;
            }

            _currentVisual = ShapeFactory.CreateShapeVisual(q);
            _currentVisual.transform.SetParent(_anchorTransform, false);
            _currentVisual.transform.localPosition = Vector3.zero;
            _currentVisual.transform.localRotation = Quaternion.identity;
            _currentVisual.AddComponent<ARObjectManipulator>().Initialize(raycastManager, Camera.main);
            _currentVisual.AddComponent<IdleRotator>();
        }
    }
}
