using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;

namespace ARGeometryGame.AR
{
    public sealed class ARBootstrapper : MonoBehaviour
    {
        [SerializeField] private bool createEventSystem = true;
        [SerializeField] private bool enablePlaneDetection = true;
        [SerializeField] private bool requestCameraPermission = true;
        [SerializeField] private bool enableFallbackCameraPreview = true;

        private ARSession _session;
        private ARCameraManager _cameraManager;
        private ARCameraBackground _cameraBackground;
        private bool _permissionRequested;
        private bool _xrStarted;
        private bool _availabilityChecked;
        private float _startupTimer;
        private ARCameraFallbackBackground _fallback;
        private bool _fallbackEnabled;

        private void Awake()
        {
            EnsureARSession();
            var origin = EnsureXROrigin();
            EnsureManagers(origin);
            CacheCameraComponents(origin.Camera);
            EnsureFallback();
            EnsureLight();
            if (createEventSystem)
            {
                UIEventSystemBootstrapper.EnsureEventSystemExists();
            }
        }

        private void Start()
        {
            StartCoroutine(BootstrapRoutine());
        }

        private void Update()
        {
            _startupTimer += Time.unscaledDeltaTime;
            ApplyPermissionState();
            ApplyFallback();

            if (!_availabilityChecked &&
                Application.platform == RuntimePlatform.Android &&
                IsCameraPermissionGranted() &&
                _startupTimer > 1f &&
                ARSession.state == ARSessionState.None)
            {
                StartCoroutine(CheckAvailabilityAndInstall());
            }
        }

        private System.Collections.IEnumerator BootstrapRoutine()
        {
            yield return EnsureXRStarted();
            TryRequestPermission();

            var waitSeconds = 0f;
            while (Application.platform == RuntimePlatform.Android &&
                   !UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera) &&
                   waitSeconds < 5f)
            {
                waitSeconds += Time.unscaledDeltaTime;
                yield return null;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                yield return CheckAvailabilityAndInstall();
            }

            ApplyPermissionState();
        }

        private System.Collections.IEnumerator CheckAvailabilityAndInstall()
        {
            if (_availabilityChecked)
            {
                yield break;
            }

            _availabilityChecked = true;

            yield return ARSession.CheckAvailability();
            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                yield return ARSession.Install();
            }
        }

        private System.Collections.IEnumerator EnsureXRStarted()
        {
            if (_xrStarted)
            {
                yield break;
            }

            var settings = XRGeneralSettings.Instance;
            if (settings == null || settings.Manager == null)
            {
                _xrStarted = true;
                yield break;
            }

            if (settings.Manager.activeLoader == null)
            {
                yield return settings.Manager.InitializeLoader();
            }

            if (settings.Manager.activeLoader != null)
            {
                settings.Manager.StartSubsystems();
            }

            _xrStarted = true;
        }

        private void EnsureARSession()
        {
            if (FindAnyObjectByType<ARSession>() != null)
            {
                return;
            }

            var go = new GameObject("AR Session");
            _session = go.AddComponent<ARSession>();
        }

        private XROrigin EnsureXROrigin()
        {
            var existing = FindAnyObjectByType<XROrigin>();
            if (existing != null)
            {
                if (existing.Camera == null)
                {
                    existing.Camera = EnsureARCamera(existing.transform);
                }
                return existing;
            }

            var go = new GameObject("XR Origin");
            var origin = go.AddComponent<XROrigin>();
            origin.CameraFloorOffsetObject = new GameObject("Camera Offset");
            origin.CameraFloorOffsetObject.transform.SetParent(origin.transform, false);
            origin.Camera = EnsureARCamera(origin.CameraFloorOffsetObject.transform);
            return origin;
        }

        private static Camera EnsureARCamera(Transform parent)
        {
            var camGo = new GameObject("AR Camera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(parent, false);

            var camera = camGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camGo.AddComponent<AudioListener>();
            var fallbackControls = camGo.AddComponent<FallbackCameraController>();
            fallbackControls.enabled = false;
            var cameraManager = camGo.AddComponent<ARCameraManager>();
            cameraManager.requestedFacingDirection = CameraFacingDirection.World;
            cameraManager.autoFocusRequested = true;
            camGo.AddComponent<ARCameraBackground>();

            return camera;
        }

        private void EnsureManagers(XROrigin origin)
        {
            var originGo = origin.gameObject;

            EnsureComponent<ARRaycastManager>(originGo);

            var planeManager = EnsureComponent<ARPlaneManager>(originGo);
            planeManager.enabled = enablePlaneDetection;

            EnsureComponent<ARAnchorManager>(originGo);
            EnsureComponent<ARPointCloudManager>(originGo);
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            if (go.TryGetComponent<T>(out var existing))
            {
                return existing;
            }

            return go.AddComponent<T>();
        }

        private void CacheCameraComponents(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            _cameraManager = camera.GetComponent<ARCameraManager>();
            _cameraBackground = camera.GetComponent<ARCameraBackground>();
        }

        private void TryRequestPermission()
        {
            if (!requestCameraPermission || _permissionRequested)
            {
                return;
            }

            _permissionRequested = true;

            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }

            if (UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                return;
            }

            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
        }

        private void ApplyPermissionState()
        {
            var granted = IsCameraPermissionGranted();

            if (_session == null)
            {
                _session = FindAnyObjectByType<ARSession>();
            }

            if (_session != null)
            {
                _session.enabled = granted;
            }

            if (_cameraManager != null)
            {
                _cameraManager.enabled = granted;
            }

            if (_cameraBackground != null)
            {
                _cameraBackground.enabled = granted;
            }
        }

        private void EnsureFallback()
        {
            if (!enableFallbackCameraPreview)
            {
                return;
            }

            if (_fallback != null)
            {
                return;
            }

            _fallback = new GameObject("AR Fallback Camera").AddComponent<ARCameraFallbackBackground>();
        }

        private void ApplyFallback()
        {
            if (!enableFallbackCameraPreview || _fallback == null)
            {
                return;
            }

            if (!IsCameraPermissionGranted())
            {
                SetFallbackEnabled(false);
                return;
            }

            var loader = XRGeneralSettings.Instance != null && XRGeneralSettings.Instance.Manager != null
                ? XRGeneralSettings.Instance.Manager.activeLoader
                : null;

            var arState = ARSession.state;
            var shouldFallback =
                loader == null ||
                arState == ARSessionState.Unsupported ||
                (arState == ARSessionState.None && _startupTimer > 3f);

            var shouldDisableFallback =
                arState == ARSessionState.SessionInitializing ||
                arState == ARSessionState.SessionTracking;

            if (shouldDisableFallback)
            {
                SetFallbackEnabled(false);
                return;
            }

            SetFallbackEnabled(shouldFallback);
        }

        private void SetFallbackEnabled(bool enabled)
        {
            if (_fallbackEnabled == enabled)
            {
                return;
            }

            _fallbackEnabled = enabled;
            if (enabled)
            {
                _fallback.StartPreview();
            }
            else
            {
                _fallback.StopPreview();
            }

            if (Camera.main != null)
            {
                var controls = Camera.main.GetComponent<FallbackCameraController>();
                if (controls != null)
                {
                    controls.enabled = enabled;
                }
            }
        }

        private static void EnsureLight()
        {
            if (FindAnyObjectByType<Light>() != null)
            {
                return;
            }

            var go = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static bool IsCameraPermissionGranted()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return true;
            }

            return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera);
        }
    }
}
