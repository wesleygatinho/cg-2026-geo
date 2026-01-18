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

        // If >= 0, only questions with this shape will be used. -1 = all shapes.
        [SerializeField] private int shapeFilter = -1;

        [SerializeField] private ARPlacementManager placementManager;
        [SerializeField] private ARAnchorPlacementManager anchorPlacementManager;
        [SerializeField] private ARPlaneVisibilityController planeVisibilityController;
        [SerializeField] private ARRaycastManager raycastManager;

        private readonly List<GeometryQuestion> _questions = new();
        private int _index;
        private Transform _anchorTransform;
        private GameObject _currentVisual;

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
        }

        private void Update()
        {
            if (!HasPlacedObject)
            {
                TryHandlePlacementTap();
            }
        }

        public void TogglePlanes()
        {
            if (planeVisibilityController == null)
            {
                return;
            }

            var anyPlaneActive = false;
            var planeManager = FindAnyObjectByType<ARPlaneManager>();
            if (planeManager != null)
            {
                foreach (var p in planeManager.trackables)
                {
                    if (p != null && p.gameObject.activeSelf)
                    {
                        anyPlaneActive = true;
                        break;
                    }
                }
            }

            planeVisibilityController.SetPlanesVisible(!anyPlaneActive);
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
                FeedbackChanged?.Invoke($"Incorreto. Tente novamente. (Dica: resultado ~ {expected:0.###} {q.unit})");
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

            var arSessionState = FindAnyObjectByType<ARSessionStateReporter>();
            if (arSessionState == null)
            {
                new GameObject("AR Session State").AddComponent<ARSessionStateReporter>();
            }
        }

        private void LoadRunQuestions()
        {
            var all = QuestionBankLoader.LoadQuestionsFromStreamingAssets(questionFileName);

            // apply shape filter if requested
            if (shapeFilter >= 0 && all != null && all.Length > 0)
            {
                var filtered = new List<GeometryQuestion>();
                foreach (var q in all)
                {
                    if ((int)q.shape == shapeFilter)
                    {
                        filtered.Add(q);
                    }
                }

                all = filtered.ToArray();
            }

            var seed = Environment.TickCount;
            var selected = QuestionSelector.SelectRandom(all, questionsPerRun, seed);
            _questions.Clear();
            _questions.AddRange(selected);
            _index = 0;
            GameSession.StartNew(_questions.Count);
        }

        // Public API so UI can set the desired shape filter at runtime.
        public void SetShapeFilter(int filter)
        {
            shapeFilter = filter; // -1 to clear filter
            LoadRunQuestions();
            RaiseQuestionChanged();
            if (_anchorTransform != null)
            {
                UpdatePlacedVisual();
            }
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

            if (ShouldUseFallbackPlacement())
            {
                TryPlaceFallback();
                return;
            }

            if (!placementManager.TryGetPlacementPose(out var pose, out var trackableId))
            {
                return;
            }

            var anchor = anchorPlacementManager.PlaceAnchor(pose, trackableId);
            if (anchor == null)
            {
                return;
            }

            _anchorTransform = anchor.transform;
            planeVisibilityController.SetPlanesVisible(false);
            UpdatePlacedVisual();
            FeedbackChanged?.Invoke("Objeto colocado. Responda a questão.");
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

            _anchorTransform.position = cam.transform.position + forward.normalized * 1.0f;
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
        }
    }
}
