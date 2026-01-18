using UnityEngine;

namespace ARGeometryGame.Gameplay
{
    public class IdleRotator : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationAxis = new Vector3(0, 1, 0);
        [SerializeField] private float speed = 15f;
        [SerializeField] private float bobFrequency = 0.5f;
        [SerializeField] private float bobAmplitude = 0.05f;

        private Vector3 _startPos;
        private bool _isInteracting;

        private void Start()
        {
            _startPos = transform.localPosition;
        }

        private void Update()
        {
            if (_isInteracting) return;

            // Rotate
            transform.Rotate(rotationAxis, speed * Time.deltaTime);

            // Bob up and down (floating effect)
            var yOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            transform.localPosition = _startPos + new Vector3(0, yOffset, 0);
        }

        public void SetInteracting(bool interacting)
        {
            _isInteracting = interacting;
        }
    }
}