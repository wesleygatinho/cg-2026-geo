using UnityEngine;
using UnityEngine.SceneManagement;
using ARGeometryGame.UI;
using ARGeometryGame.AR;
using ARGeometryGame.Gameplay;

namespace ARGeometryGame.Core
{
    public sealed class AppRoot : MonoBehaviour
    {
        private void OnEnable()
        {
            SceneManager.activeSceneChanged += HandleActiveSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
        }

        private void Start()
        {
            InstallFor(SceneManager.GetActiveScene().name);
        }

        private void HandleActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            InstallFor(newScene.name);
        }

        private void InstallFor(string sceneName)
        {
            if (sceneName == SceneNames.Boot)
            {
                if (FindAnyObjectByType<BootFlowController>() == null)
                {
                    new GameObject("Boot Flow").AddComponent<BootFlowController>();
                }
                return;
            }

            if (sceneName == SceneNames.Menu)
            {
                if (FindAnyObjectByType<MenuFlowController>() == null)
                {
                    new GameObject("Menu Flow").AddComponent<MenuFlowController>();
                }
                if (FindAnyObjectByType<MenuUIInstaller>() == null)
                {
                    new GameObject("Menu UI").AddComponent<MenuUIInstaller>();
                }
                return;
            }

            if (sceneName == SceneNames.ARGameplay)
            {
                if (FindAnyObjectByType<ARGameplayFlowController>() == null)
                {
                    new GameObject("AR Gameplay Flow").AddComponent<ARGameplayFlowController>();
                }
                if (FindAnyObjectByType<ARBootstrapper>() == null)
                {
                    new GameObject("AR Bootstrap").AddComponent<ARBootstrapper>();
                }
                if (FindAnyObjectByType<ARGameplayQuizController>() == null)
                {
                    new GameObject("AR Quiz").AddComponent<ARGameplayQuizController>();
                }
                if (FindAnyObjectByType<ARGameplayUIInstaller>() == null)
                {
                    new GameObject("AR Gameplay UI").AddComponent<ARGameplayUIInstaller>();
                }
                return;
            }

            if (sceneName == SceneNames.Results)
            {
                if (FindAnyObjectByType<ResultsFlowController>() == null)
                {
                    new GameObject("Results Flow").AddComponent<ResultsFlowController>();
                }
                if (FindAnyObjectByType<ResultsUIInstaller>() == null)
                {
                    new GameObject("Results UI").AddComponent<ResultsUIInstaller>();
                }
            }
        }
    }
}
