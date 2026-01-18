using UnityEngine;
using UnityEngine.EventSystems;

namespace ARGeometryGame.AR
{
    public sealed class FallbackCameraController : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 0.12f;
        [SerializeField] private float moveSpeed = 0.5f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;
        [SerializeField] private float pinchMoveSpeed = 0.02f;

        private Camera _camera;
        private Vector2 _lastTouch;
        private float _lastPinchDistance;
        private float _yaw;
        private float _pitch;
        private Vector2 _lastPanTouch;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            var euler = transform.rotation.eulerAngles;
            _yaw = euler.y;
            _pitch = NormalizePitch(euler.x);
            
            if (SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = true;
            }
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

            if (SystemInfo.supportsGyroscope)
            {
                // Apply gyro rotation
                // Transform gyro attitude to Unity camera coordinate system
                var attitude = Input.gyro.attitude;
                // Remap: Gyro right-handed to Unity left-handed
                var rot = new Quaternion(attitude.x, attitude.y, -attitude.z, -attitude.w);
                // Rotate 90 degrees around X to match landscape orientation usually
                transform.localRotation = Quaternion.Euler(90, 0, 0) * rot;
            }
            else
            {
                // Only use touch if no gyro, or combine them? 
                // For now, let's keep touch as an override or additional input
                HandleTouch();
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseAndKeyboard();
#endif
        }

        private void HandleMouseAndKeyboard()
        {
            // Rotation
            if (Input.GetMouseButton(1))
            {
                _yaw += Input.GetAxis("Mouse X") * rotationSpeed * 20f;
                _pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * 20f;
                _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
                transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }

            // Movement
            var move = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) move += transform.forward;
            if (Input.GetKey(KeyCode.S)) move -= transform.forward;
            if (Input.GetKey(KeyCode.A)) move -= transform.right;
            if (Input.GetKey(KeyCode.D)) move += transform.right;
            if (Input.GetKey(KeyCode.Q)) move -= transform.up;
            if (Input.GetKey(KeyCode.E)) move += transform.up;

            if (move != Vector3.zero)
            {
                transform.position += move.normalized * moveSpeed * Time.deltaTime * 5f;
            }
        }

        private void HandleTouch()
        {
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
                var center = (t0.position + t1.position) * 0.5f;

                if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
                {
                    _lastPinchDistance = dist;
                    _lastPanTouch = center;
                    return;
                }

                // Pinch for forward/backward
                var pinchDelta = dist - _lastPinchDistance;
                _lastPinchDistance = dist;

                if (Mathf.Abs(pinchDelta) > 0.1f)
                {
                    transform.position += transform.forward * pinchDelta * pinchMoveSpeed;
                }

                // Pan (Move vertically/horizontally)
                var panDelta = center - _lastPanTouch;
                _lastPanTouch = center;

                // Adjust pan sensitivity based on screen size or arbitrary factor
                var panSpeed = 0.005f; 
                transform.position -= transform.right * panDelta.x * panSpeed;
                transform.position -= transform.up * panDelta.y * panSpeed;
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

