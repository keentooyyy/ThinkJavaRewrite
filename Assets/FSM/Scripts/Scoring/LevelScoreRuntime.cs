namespace GameScoring
{
    /// <summary>
    /// Runtime holder for last computed score/stars from the LevelScoreFSM.
    /// UI can read these values on ScoreComputed event to avoid recomputation.
    /// </summary>
    public static class LevelScoreRuntime
    {
        public static int LastScore { get; set; }
        public static int LastStars { get; set; }

        public static void Reset()
        {
            LastScore = 0;
            LastStars = 0;
        }
    }
}

