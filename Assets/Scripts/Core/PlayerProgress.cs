using UnityEngine;

namespace ARGeometryGame.Core
{
    public static class PlayerProgress
    {
        private const string BestScoreKey = "best_score";

        public static int GetBestScore()
        {
            return PlayerPrefs.GetInt(BestScoreKey, 0);
        }

        public static bool TrySetBestScore(int score)
        {
            var best = GetBestScore();
            if (score <= best)
            {
                return false;
            }

            PlayerPrefs.SetInt(BestScoreKey, score);
            PlayerPrefs.Save();
            return true;
        }
    }
}

