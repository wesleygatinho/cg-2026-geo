using UnityEngine;

namespace ARGeometryGame.Core
{
    public sealed class BootFlowController : MonoBehaviour
    {
        [SerializeField] private float delaySeconds = 0.2f;

        private float _elapsed;

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed < delaySeconds)
            {
                return;
            }

            SceneNavigator.LoadMenu();
        }
    }
}

