using UnityEngine;

namespace ARGeometryGame.Gameplay
{
    /// <summary>
    /// Componente que faz o objeto seguir a câmera mantendo-se sempre à frente dela.
    /// Usado no modo fallback quando AR não está disponível.
    /// </summary>
    public sealed class CameraFollower : MonoBehaviour
    {
        [SerializeField] private float distance = 1.5f;
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private bool useSmoothing = true;

        private Camera _targetCamera;
        private Vector3 _targetPosition;

        private void Awake()
        {
            _targetCamera = Camera.main;
            if (_targetCamera == null)
            {
                _targetCamera = FindAnyObjectByType<Camera>();
            }
        }

        private void Update()
        {
            if (_targetCamera == null)
            {
                _targetCamera = Camera.main;
                if (_targetCamera == null)
                {
                    return;
                }
            }

            // Calcular posição alvo à frente da câmera
            var forward = _targetCamera.transform.forward;
            var flatForward = new Vector3(forward.x, 0f, forward.z);
            if (flatForward.sqrMagnitude < 0.001f)
            {
                flatForward = Vector3.forward;
            }

            _targetPosition = _targetCamera.transform.position + flatForward.normalized * distance;
            _targetPosition.y = _targetCamera.transform.position.y; // Manter na mesma altura da câmera

            // Atualizar posição
            if (useSmoothing)
            {
                transform.position = Vector3.Lerp(transform.position, _targetPosition, followSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = _targetPosition;
            }

            // Atualizar rotação para olhar na direção da câmera
            var lookDirection = flatForward.normalized;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            }
        }
    }
}
