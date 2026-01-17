using UnityEngine;
using UnityEngine.EventSystems;

namespace ARGeometryGame.AR
{
    public sealed class FallbackCameraController : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 0.12f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;
        [SerializeField] private float pinchZoomSpeed = 0.06f;
        [SerializeField] private float minFov = 30f;
        [SerializeField] private float maxFov = 75f;

        private Camera _camera;
        private Vector2 _lastTouch;
        private float _lastPinchDistance;
        private float _yaw;
        private float _pitch;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            var euler = transform.rotation.eulerAngles;
            _yaw = euler.y;
            _pitch = NormalizePitch(euler.x);
        }

        private void Update()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
                if (_camera == null)
                {
                    return;
                }
            }

            if (Input.touchCount == 1)
            {
                var t = Input.GetTouch(0);
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
                {
                    return;
                }

                if (t.phase == TouchPhase.Began)
                {
                    _lastTouch = t.position;
                    return;
                }

                if (t.phase == TouchPhase.Moved)
                {
                    var delta = t.position - _lastTouch;
                    _lastTouch = t.position;

                    _yaw += delta.x * rotationSpeed;
                    _pitch -= delta.y * rotationSpeed;
                    _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

                    transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
                }

                return;
            }

            if (Input.touchCount >= 2)
            {
                var t0 = Input.GetTouch(0);
                var t1 = Input.GetTouch(1);

                if (EventSystem.current != null &&
                    (EventSystem.current.IsPointerOverGameObject(t0.fingerId) || EventSystem.current.IsPointerOverGameObject(t1.fingerId)))
                {
                    return;
                }

                var dist = Vector2.Distance(t0.position, t1.position);
                if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
                {
                    _lastPinchDistance = dist;
                    return;
                }

                var delta = dist - _lastPinchDistance;
                _lastPinchDistance = dist;

                var fov = _camera.fieldOfView;
                fov -= delta * pinchZoomSpeed;
                _camera.fieldOfView = Mathf.Clamp(fov, minFov, maxFov);
            }
        }

        private static float NormalizePitch(float pitch)
        {
            if (pitch > 180f)
            {
                pitch -= 360f;
            }
            return pitch;
        }
    }
}

