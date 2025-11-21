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

        public GameSaveData() 
        {
        }
    }
}

