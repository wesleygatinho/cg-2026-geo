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

            if (raycastManager == null)
            {
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

        public bool TryGetPlacementPose(out Pose pose)
        {
            if (!TryGetPlacementHit(out var hit))
            {
                pose = default;
                return false;
            }

            pose = hit.pose;
            return true;
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
