using System;
using UnityEngine;

namespace GameProgress
{
    /// <summary>
    /// Achievement save data structure
    /// </summary>
    [Serializable]
    public class AchievementSaveData
    {
        public string title;
        public string description;
        public bool unlocked;

        public AchievementSaveData() { }

        public AchievementSaveData(string title, string description, bool unlocked)
        {
            this.title = title;
            this.description = description;
            this.unlocked = unlocked;
        }
    }
}

