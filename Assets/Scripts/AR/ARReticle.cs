using UnityEngine;

namespace ARGeometryGame.AR
{
    public sealed class ARReticle : MonoBehaviour
    {
        [SerializeField] private ARPlacementManager placementManager;
        [SerializeField] private GameObject visual;

        private void Awake()
        {
            if (visual == null)
            {
                visual = gameObject;
            }
        }

        private void Update()
        {
            if (placementManager == null)
            {
                visual.SetActive(false);
                return;
            }

            if (!placementManager.TryGetPlacementPose(out var pose))
            {
                visual.SetActive(false);
                return;
            }

            visual.SetActive(true);
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
    }
}

