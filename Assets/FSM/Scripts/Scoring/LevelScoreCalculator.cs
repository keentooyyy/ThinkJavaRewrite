using System.Collections.Generic;

namespace GameScoring
{
    /// <summary>
    /// Local score/stars calculator based on elapsed time thresholds.
    /// Higher time => higher score.
    /// </summary>
    public static class LevelScoreCalculator
    {
        // (thresholdSeconds, score)
        private static readonly List<(float threshold, int score)> LEVEL_SCORE_THRESHOLDS = new()
        {
            (180f, 100),
            (120f, 70),
            (60f, 40),
            (1f, 10),
        };

        public static int CalcLevelScore(float elapsedSeconds)
        {
            if (elapsedSeconds <= 0f) return 0;
            foreach (var (threshold, score) in LEVEL_SCORE_THRESHOLDS)
            {
                if (elapsedSeconds >= threshold)
                    return score;
            }
            return 0;
        }

        public static int CalcLevelStars(float elapsedSeconds)
        {
            int score = CalcLevelScore(elapsedSeconds);
            for (int i = 0; i < LEVEL_SCORE_THRESHOLDS.Count; i++)
            {
                var (_, s) = LEVEL_SCORE_THRESHOLDS[i];
                if (score == s)
                {
                    return i switch
                    {
                        0 => 3,
                        1 => 2,
                        2 => 2,
                        3 => 1,
                        _ => 0
                    };
                }
            }
            return 0;
        }
    }
}

