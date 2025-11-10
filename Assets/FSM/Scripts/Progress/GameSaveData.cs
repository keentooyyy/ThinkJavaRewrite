using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameProgress
{
    /// <summary>
    /// Complete game save data structure matching JSON format
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public Dictionary<string, LevelData> levels = new Dictionary<string, LevelData>();
        public Dictionary<string, AchievementSaveData> achievements = new Dictionary<string, AchievementSaveData>();

        // Metadata for sync tracking
        public long lastModifiedTimestamp = 0; // Unix timestamp in milliseconds

        public GameSaveData() 
        {
            lastModifiedTimestamp = GetCurrentTimestamp();
        }

        private long GetCurrentTimestamp()
        {
            return System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public void UpdateTimestamp()
        {
            lastModifiedTimestamp = GetCurrentTimestamp();
        }
    }
}

