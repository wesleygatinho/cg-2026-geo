using UnityEngine;

namespace ARGeometryGame.Core
{
    public sealed class MenuFlowController : MonoBehaviour
    {
        public void StartGame()
        {
            SceneNavigator.LoadARGameplay();
        }
    }
}

