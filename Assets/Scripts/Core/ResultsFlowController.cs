using UnityEngine;

namespace ARGeometryGame.Core
{
    public sealed class ResultsFlowController : MonoBehaviour
    {
        public void BackToMenu()
        {
            SceneNavigator.LoadMenu();
        }
    }
}

