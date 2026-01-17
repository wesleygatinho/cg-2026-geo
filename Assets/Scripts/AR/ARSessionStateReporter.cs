using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ARGeometryGame.AR
{
    public sealed class ARSessionStateReporter : MonoBehaviour
    {
        public event Action<ARSessionState> SessionStateChanged;
        public event Action<NotTrackingReason> NotTrackingReasonChanged;

        private ARSessionState _lastState;
        private NotTrackingReason _lastReason;

        private void Update()
        {
            var state = ARSession.state;
            if (state != _lastState)
            {
                _lastState = state;
                SessionStateChanged?.Invoke(state);
            }

            var reason = ARSession.notTrackingReason;
            if (reason != _lastReason)
            {
                _lastReason = reason;
                NotTrackingReasonChanged?.Invoke(reason);
            }
        }
    }
}
