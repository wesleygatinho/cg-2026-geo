using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ARGeometryGame.AR
{
    public sealed class ARPlaneVisibilityController : MonoBehaviour
    {
        [SerializeField] private ARPlaneManager planeManager;

        public void SetPlanesVisible(bool visible)
        {
            if (planeManager == null)
            {
                planeManager = FindAnyObjectByType<ARPlaneManager>();
            }

            if (planeManager == null)
            {
                return;
            }

            planeManager.enabled = visible;

            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(visible);
            }
        }
    }
}
