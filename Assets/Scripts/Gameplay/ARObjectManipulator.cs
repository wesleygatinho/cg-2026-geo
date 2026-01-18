using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARGeometryGame.Gameplay
{
    public sealed class ARObjectManipulator : MonoBehaviour
    {
        private ARRaycastManager _raycastManager;
        private Camera _camera;

        private bool _selected;
        private Vector2 _lastTouch0;
        private float _lastPinchDistance;
        private float _lastTwistAngle;

        private static readonly List<ARRaycastHit> Hits = new();

        public void Initialize(ARRaycastManager raycastManager, Camera camera)
        {
            _raycastManager = raycastManager;
            _camera = camera;
        }

        private void Awake()
        {
            EnsureCollider();
        }

        private void Update()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (Input.touchCount == 1)
            {
                HandleOneTouch(Input.GetTouch(0));
                return;
            }

            if (Input.touchCount >= 2)
            {
                HandleTwoTouches(Input.GetTouch(0), Input.GetTouch(1));
                return;
            }
        }

        private void HandleOneTouch(Touch touch)
        {
            if (touch.phase == TouchPhase.Began)
            {
                _selected = HitThisObject(touch.position);
                _lastTouch0 = touch.position;
                if (_selected)
                {
                    GetComponent<IdleRotator>()?.SetInteracting(true);
                }
                return;
            }

            if (!_selected)
            {
                return;
            }

            if (touch.phase == TouchPhase.Moved)
            {
                if (_raycastManager != null)
                {
                    if (_raycastManager.Raycast(touch.position, Hits, TrackableType.PlaneWithinPolygon))
                    {
                        var pose = Hits[0].pose;
                        transform.position = pose.position;
                    }
                }
                else if (_camera != null)
                {
                    var distance = Vector3.Dot(transform.position - _camera.transform.position, _camera.transform.forward);
                    if (distance < 0.2f)
                    {
                        distance = 1.0f;
                    }

                    var ray = _camera.ScreenPointToRay(touch.position);
                    transform.position = ray.origin + ray.direction * distance;
                }
                _lastTouch0 = touch.position;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                _selected = false;
                GetComponent<IdleRotator>()?.SetInteracting(false);
            }
        }

        private void HandleTwoTouches(Touch t0, Touch t1)
        {
            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                _selected = HitThisObject(t0.position) || HitThisObject(t1.position);
                if (_selected)
                {
                    GetComponent<IdleRotator>()?.SetInteracting(true);
                }
            }

            if (!_selected)
            {
                return;
            }

            var p0 = t0.position;
            var p1 = t1.position;

            var pinchDistance = Vector2.Distance(p0, p1);
            var twistAngle = Mathf.Atan2(p1.y - p0.y, p1.x - p0.x) * Mathf.Rad2Deg;

            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                _lastPinchDistance = pinchDistance;
                _lastTwistAngle = twistAngle;
                return;
            }

            var pinchDelta = pinchDistance - _lastPinchDistance;
            var scaleFactor = 1f + pinchDelta / 500f;
            var nextScale = transform.localScale * scaleFactor;
            var clamped = Mathf.Clamp(nextScale.x, 0.05f, 2.0f);
            transform.localScale = new Vector3(clamped, clamped, clamped);
            _lastPinchDistance = pinchDistance;

            var angleDelta = twistAngle - _lastTwistAngle;
            transform.Rotate(0f, -angleDelta, 0f, Space.World);
            _lastTwistAngle = twistAngle;

            if (t0.phase == TouchPhase.Ended || t1.phase == TouchPhase.Ended || t0.phase == TouchPhase.Canceled || t1.phase == TouchPhase.Canceled)
            {
                _selected = false;
                GetComponent<IdleRotator>()?.SetInteracting(false);
            }
        }

        private bool HitThisObject(Vector2 screenPosition)
        {
            if (_camera == null)
            {
                return false;
            }

            var ray = _camera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out var hit))
            {
                return false;
            }

            return hit.transform == transform || hit.transform.IsChildOf(transform);
        }

        private void EnsureCollider()
        {
            if (GetComponentInChildren<Collider>() != null)
            {
                return;
            }

            var mr = GetComponentInChildren<MeshRenderer>();
            if (mr != null)
            {
                var bc = mr.gameObject.AddComponent<BoxCollider>();
                bc.center = Vector3.zero;
            }
        }
    }
}
