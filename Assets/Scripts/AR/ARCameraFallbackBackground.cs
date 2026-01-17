using System.Linq;
using UnityEngine;

namespace ARGeometryGame.AR
{
    public sealed class ARCameraFallbackBackground : MonoBehaviour
    {
        private WebCamTexture _webcam;
        private Camera _camera;
        private Transform _quad;
        private MeshRenderer _quadRenderer;
        private Material _material;
        private bool _isFrontFacing;

        public bool IsRunning => _webcam != null && _webcam.isPlaying;

        private void Awake()
        {
            EnsureQuad();
        }

        public void StartPreview()
        {
            if (IsRunning)
            {
                return;
            }

            EnsureQuad();
            if (_camera == null || _quadRenderer == null)
            {
                return;
            }

            if (_webcam == null)
            {
                var device = WebCamTexture.devices.FirstOrDefault(d => !d.isFrontFacing);
                if (string.IsNullOrWhiteSpace(device.name))
                {
                    device = WebCamTexture.devices.Length > 0 ? WebCamTexture.devices[0] : default;
                }

                _isFrontFacing = device.name != null && device.isFrontFacing;
                _webcam = device.name != null ? new WebCamTexture(device.name, 1280, 720, 30) : new WebCamTexture(1280, 720, 30);
                _material.mainTexture = _webcam;
            }

            _quadRenderer.enabled = true;
            _webcam.Play();
        }

        public void StopPreview()
        {
            if (_webcam == null)
            {
                return;
            }

            if (_webcam.isPlaying)
            {
                _webcam.Stop();
            }

            if (_quadRenderer != null)
            {
                _quadRenderer.enabled = false;
            }
        }

        private void Update()
        {
            if (_webcam == null || !_webcam.isPlaying)
            {
                return;
            }

            UpdateQuadTransform();
            UpdateMaterialUV();
        }

        private void EnsureQuad()
        {
            if (_quad != null)
            {
                return;
            }

            _camera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
            if (_camera == null)
            {
                return;
            }

            var quadGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quadGo.name = "Fallback Camera Quad";
            if (quadGo.TryGetComponent<Collider>(out var col))
            {
                Destroy(col);
            }

            quadGo.transform.SetParent(_camera.transform, false);
            _quad = quadGo.transform;

            _quadRenderer = quadGo.GetComponent<MeshRenderer>();

            var shader = Shader.Find("Unlit/Texture") ?? Shader.Find("Sprites/Default");
            _material = new Material(shader);
            _quadRenderer.sharedMaterial = _material;
            _quadRenderer.enabled = false;
        }

        private void UpdateQuadTransform()
        {
            if (_camera == null || _quad == null)
            {
                return;
            }

            var distance = Mathf.Clamp(_camera.farClipPlane * 0.9f, 2f, 50f);
            var frustumHeight = 2f * distance * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var frustumWidth = frustumHeight * _camera.aspect;

            _quad.localPosition = new Vector3(0f, 0f, distance);
            _quad.localRotation = Quaternion.Euler(0f, 0f, -_webcam.videoRotationAngle);

            _quad.localScale = new Vector3(frustumWidth, frustumHeight, 1f);
        }

        private void UpdateMaterialUV()
        {
            if (_material == null || _camera == null)
            {
                return;
            }

            var w = _webcam.width;
            var h = _webcam.height;
            if (w <= 16 || h <= 16)
            {
                return;
            }

            var rotated = _webcam.videoRotationAngle == 90 || _webcam.videoRotationAngle == 270;
            var texAspect = rotated ? (float)h / w : (float)w / h;
            var camAspect = _camera.aspect;

            var uScale = 1f;
            var vScale = 1f;
            var uOffset = 0f;
            var vOffset = 0f;

            if (texAspect > camAspect)
            {
                uScale = camAspect / texAspect;
                uOffset = (1f - uScale) * 0.5f;
            }
            else
            {
                vScale = texAspect / camAspect;
                vOffset = (1f - vScale) * 0.5f;
            }

            var flipY = _webcam.videoVerticallyMirrored;
            var flipX = _isFrontFacing;

            var finalUScale = flipX ? -uScale : uScale;
            var finalVScale = flipY ? -vScale : vScale;
            var finalUOffset = flipX ? uOffset + uScale : uOffset;
            var finalVOffset = flipY ? vOffset + vScale : vOffset;

            _material.mainTextureScale = new Vector2(finalUScale, finalVScale);
            _material.mainTextureOffset = new Vector2(finalUOffset, finalVOffset);
        }
    }
}
