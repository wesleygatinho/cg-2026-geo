using System;

namespace ARGeometryGame.Core
{
    public static class GameSession
    {
        public static int TotalQuestions { get; private set; }
        public static int CorrectAnswers { get; private set; }
        public static int Attempts { get; private set; }
        public static float ElapsedSeconds { get; private set; }
        public static int Score { get; private set; }

        public static event Action SessionChanged;

        public static void StartNew(int totalQuestions)
        {
            TotalQuestions = Math.Max(0, totalQuestions);
            CorrectAnswers = 0;
            Attempts = 0;
            ElapsedSeconds = 0;
            Score = 0;
            SessionChanged?.Invoke();
        }

        public static void RegisterAttempt(bool correct)
        {
            Attempts++;
            if (correct)
            {
                CorrectAnswers++;
            }
            SessionChanged?.Invoke();
        }

        public static void Tick(float deltaTime)
        {
            if (deltaTime <= 0)
            {
                return;
            }
            ElapsedSeconds += deltaTime;
            RecomputeScore();
        }

        public static void RecomputeScore()
        {
            var wrong = Math.Max(0, Attempts - CorrectAnswers);
            var timePenalty = (int)Math.Floor(ElapsedSeconds * 2f);
            var raw = CorrectAnswers * 100 - wrong * 10 - timePenalty;
            Score = Math.Max(0, raw);
            SessionChanged?.Invoke();
        }
    }
}
