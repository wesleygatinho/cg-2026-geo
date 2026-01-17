using UnityEngine;

namespace ARGeometryGame.Core
{
    public sealed class ARGameplayFlowController : MonoBehaviour
    {
        [SerializeField] private int questionsPerRun = 10;

        private void Start()
        {
            GameSession.StartNew(questionsPerRun);
        }

        private void Update()
        {
            GameSession.Tick(Time.deltaTime);
        }

        public void FinishRun()
        {
            GameSession.RecomputeScore();
            PlayerProgress.TrySetBestScore(GameSession.Score);
            SceneNavigator.LoadResults();
        }
    }
}
