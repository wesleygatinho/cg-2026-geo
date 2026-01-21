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

        // Visual selection ring shown while the object is selected
        private GameObject _selectionRing;

        public void Initialize(ARRaycastManager raycastManager, Camera camera)
        {
            _raycastManager = raycastManager;
            _camera = camera;
        }

        private void Awake()
        {
            EnsureCollider();
            CreateSelectionRing();
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
                SetSelection(_selected);
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
                    else if (_camera != null)
                    {
                        // Project touch onto horizontal plane at object's current height so user can reposition even when AR raycast fails
                        var ray = _camera.ScreenPointToRay(touch.position);
                        var plane = new Plane(Vector3.up, transform.position);
                        if (plane.Raycast(ray, out var enter))
                        {
                            transform.position = ray.GetPoint(enter);
                        }
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
                    // Prefer projecting to a horizontal plane at the object's current Y to allow intuitive placement
                    var plane = new Plane(Vector3.up, transform.position);
                    if (plane.Raycast(ray, out var enter))
                    {
                        transform.position = ray.GetPoint(enter);
                    }
                    else
                    {
                        transform.position = ray.origin + ray.direction * distance;
                    }
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
            UpdateSelectionRingScale();
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
            // First try physics raycast
            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    return true;
                }
            }

            // If single hit failed try all hits and check children
            var hits = Physics.RaycastAll(ray);
            foreach (var h in hits)
            {
                if (h.transform == transform || h.transform.IsChildOf(transform))
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureCollider()
        {
            // If any collider exists on this object or children we're done
            if (GetComponentInChildren<Collider>() != null)
            {
                return;
            }

            // Try to compute bounds from renderers and add a BoxCollider on the root for easier picking
            var renderers = GetComponentsInChildren<Renderer>();
            if (renderers != null && renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                for (var i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }

                var bc = gameObject.AddComponent<BoxCollider>();
                // Set center relative to local transform
                var localCenter = transform.InverseTransformPoint(bounds.center);
                bc.center = localCenter;
                var localSize = transform.InverseTransformVector(bounds.size);
                bc.size = new Vector3(Mathf.Max(0.01f, Mathf.Abs(localSize.x)), Mathf.Max(0.01f, Mathf.Abs(localSize.y)), Mathf.Max(0.01f, Mathf.Abs(localSize.z)));
                return;
            }

            // Fallback: add a small box collider centered at origin
            var fallback = gameObject.AddComponent<BoxCollider>();
            fallback.center = Vector3.zero;
            fallback.size = Vector3.one * 0.2f;
        }

        private void CreateSelectionRing()
        {
            // Remove existing if any
            if (_selectionRing != null)
            {
                Destroy(_selectionRing);
            }

            var bc = GetComponent<BoxCollider>();
            var radius = 0.5f;
            if (bc != null)
            {
                radius = Mathf.Max(0.05f, Mathf.Max(bc.size.x, bc.size.z) * 0.6f);
            }

            _selectionRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _selectionRing.name = "SelectionRing";
            _selectionRing.transform.SetParent(transform, false);
            _selectionRing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            _selectionRing.transform.localPosition = new Vector3(0f, -0.01f, 0f);
            _selectionRing.transform.localScale = new Vector3(radius, 0.002f, radius);
            // remove collider so it doesn't interfere with touches
            var col = _selectionRing.GetComponent<Collider>();
            if (col != null) Destroy(col);

            var ringMr = _selectionRing.GetComponent<MeshRenderer>();

            // Determine base color from the shape's material if available
            Color baseColor = new Color(1f, 0.85f, 0.2f);
            var shapeMr = GetComponentInChildren<MeshRenderer>();
            if (shapeMr != null && shapeMr.sharedMaterial != null)
            {
                baseColor = shapeMr.sharedMaterial.color;
            }

            // Make a brighter, slightly translucent ring color derived from the shape color
            var ringColor = Color.Lerp(baseColor, Color.white, 0.45f);
            ringColor.a = 0.4f;

            var shader = Shader.Find("Standard");
            Material mat;
            if (shader != null)
            {
                mat = new Material(shader);
                // setup transparent blending
                mat.SetFloat("_Mode", 3f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            else
            {
                mat = new Material(Shader.Find("Sprites/Default"));
            }

            mat.color = ringColor;
            // subtle emission based on original shape color for visibility
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", baseColor * 0.55f);
            ringMr.material = mat;
            _selectionRing.SetActive(false);
        }

        private void SetSelection(bool on)
        {
            if (_selectionRing != null)
            {
                _selectionRing.SetActive(on);
                if (on) UpdateSelectionRingScale();
            }
        }

        private void UpdateSelectionRingScale()
        {
            if (_selectionRing == null) return;
            var bc = GetComponent<BoxCollider>();
            var radius = 0.5f;
            if (bc != null)
            {
                radius = Mathf.Max(0.05f, Mathf.Max(bc.size.x, bc.size.z) * 0.6f) * transform.localScale.x;
            }
            _selectionRing.transform.localScale = new Vector3(radius, 0.002f, radius);
        }
    }
}
