using UnityEngine.SceneManagement;

namespace ARGeometryGame.Core
{
    public static class SceneNavigator
    {
        public static void LoadBoot() => SceneManager.LoadScene(SceneNames.Boot);
        public static void LoadMenu() => SceneManager.LoadScene(SceneNames.Menu);
        public static void LoadARGameplay() => SceneManager.LoadScene(SceneNames.ARGameplay);
        public static void LoadResults() => SceneManager.LoadScene(SceneNames.Results);
    }
}

