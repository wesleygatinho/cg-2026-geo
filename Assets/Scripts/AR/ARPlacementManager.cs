using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARGeometryGame.AR
{
    public sealed class ARPlacementManager : MonoBehaviour
    {
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private Camera arCamera;
        [SerializeField] private TrackableType trackableTypes = TrackableType.PlaneWithinPolygon;

        private static readonly List<ARRaycastHit> Hits = new();

        public bool TryGetPlacementHit(out ARRaycastHit hit)
        {
            hit = default;

            if (raycastManager == null)
            {
                raycastManager = FindAnyObjectByType<ARRaycastManager>();
            }

            // Fallback for when AR is unsupported/not ready: Raycast against a virtual floor
            if (raycastManager == null || ARSession.state == ARSessionState.Unsupported || ARSession.state == ARSessionState.None)
            {
                 if (arCamera == null) arCamera = Camera.main;
                 if (arCamera == null) return false;

                 // Virtual floor at y = -1.5 (approximate height of holding phone standing up) relative to camera?
                 // No, usually camera is at (0,0,0) and floor is at -1.5. 
                 // Or we can just do a plane at y = -1.5 in world space if the camera is at (0,0,0).
                 // Better: Create a mathematical plane at y = arCamera.transform.position.y - 1.5f;
                 
                 var planeY = arCamera.transform.position.y - 1.5f;
                 var plane = new Plane(Vector3.up, new Vector3(0, planeY, 0));
                 var ray = arCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                 
                 if (plane.Raycast(ray, out var dist))
                 {
                     // Create a fake hit
                     var point = ray.GetPoint(dist);
                     // We can't easily create an ARRaycastHit structure because it's internal/struct, 
                     // but we can't return it here easily if we want to be strict.
                     // However, this method returns ARRaycastHit. 
                     // Since we can't construct ARRaycastHit easily with public API in some versions, 
                     // we might need to change the API of this manager or handle fallback differently.
                     
                     // ACTUALLY: ARRaycastHit is a struct, we might be able to default it but we can't set pose.
                     // So we should rely on TryGetPlacementPose instead for fallback.
                     return false;
                 }
                 return false;
            }

            if (arCamera == null)
            {
                arCamera = Camera.main;
            }

            var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (!raycastManager.Raycast(screenCenter, Hits, trackableTypes))
            {
                return false;
            }

            hit = Hits[0];
            return true;
        }

        /// <summary>
        /// Raycast na posição específica da tela (para usar posição do toque)
        /// </summary>
        public bool TryGetPlacementHitAtPosition(Vector2 screenPosition, out ARRaycastHit hit)
        {
            hit = default;

            if (raycastManager == null)
            {
                raycastManager = FindAnyObjectByType<ARRaycastManager>();
            }

            if (raycastManager == null)
            {
                return false;
            }

            if (!raycastManager.Raycast(screenPosition, Hits, trackableTypes))
            {
                return false;
            }

            hit = Hits[0];
            return true;
        }

        public bool TryGetPlacementPose(out Pose pose)
        {
            if (TryGetPlacementHit(out var hit))
            {
                pose = hit.pose;
                return true;
            }

            // Fallback Logic
            if (ARSession.state == ARSessionState.Unsupported || ARSession.state == ARSessionState.None || raycastManager == null)
            {
                if (arCamera == null) arCamera = Camera.main;
                if (arCamera != null)
                {
                    // Simulate a hit on a floor 1.5m below camera
                    var planeY = arCamera.transform.position.y - 1.5f;
                    var plane = new Plane(Vector3.up, new Vector3(0, planeY, 0));
                    var ray = arCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

                    if (plane.Raycast(ray, out var dist))
                    {
                        // Limit distance to avoid placing things too far
                        if (dist > 10f) dist = 10f; 
                        var point = ray.GetPoint(dist);
                        pose = new Pose(point, Quaternion.identity);
                        return true;
                    }
                    
                    // If looking at horizon/sky, place it floating in front
                    pose = new Pose(arCamera.transform.position + arCamera.transform.forward * 2.0f, Quaternion.identity);
                    return true;
                }
            }

            pose = default;
            return false;
        }

        public bool TryGetPlacementPose(out Pose pose, out TrackableId trackableId)
        {
            if (!TryGetPlacementHit(out var hit))
            {
                pose = default;
                trackableId = default;
                return false;
            }

            pose = hit.pose;
            trackableId = hit.trackableId;
            return true;
        }

        public bool TryGetPlacementPoseDeprecated(out Pose pose)
        {
            if (raycastManager == null)
            {
                raycastManager = FindAnyObjectByType<ARRaycastManager>();
            }

            if (raycastManager == null)
            {
                pose = default;
                return false;
            }

            if (arCamera == null)
            {
                arCamera = Camera.main;
            }

            var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (!raycastManager.Raycast(screenCenter, Hits, trackableTypes))
            {
                pose = default;
                return false;
            }

            pose = Hits[0].pose;
            return true;
        }
    }
}
