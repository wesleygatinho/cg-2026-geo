using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARGeometryGame.AR
{
    public sealed class ARAnchorPlacementManager : MonoBehaviour
    {
        [SerializeField] private ARAnchorManager anchorManager;
        [SerializeField] private ARPlaneManager planeManager;

        public ARAnchor PlaceAnchor(Pose pose, TrackableId trackableId)
        {
            if (anchorManager == null)
            {
                anchorManager = FindAnyObjectByType<ARAnchorManager>();
            }

            if (anchorManager == null)
            {
                return CreateStandaloneAnchor(pose);
            }

            if (planeManager == null)
            {
                planeManager = FindAnyObjectByType<ARPlaneManager>();
            }

            if (planeManager != null)
            {
                var plane = planeManager.GetPlane(trackableId);
                if (plane != null)
                {
                    var attached = anchorManager.AttachAnchor(plane, pose);
                    if (attached != null)
                    {
                        return attached;
                    }
                }
            }

            return CreateStandaloneAnchor(pose);
        }

        private static ARAnchor CreateStandaloneAnchor(Pose pose)
        {
            var go = new GameObject("Anchor");
            go.transform.SetPositionAndRotation(pose.position, pose.rotation);
            return go.AddComponent<ARAnchor>();
        }
    }
}
