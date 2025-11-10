using System;
using UnityEngine;

namespace GameProgress
{
    /// <summary>
    /// Level progress data (without localScore/localStars as per requirements)
    /// </summary>
    [Serializable]
    public class LevelData
    {
        public float bestTime = 0f;
        public bool unlocked = false;
        public float currentTime = 0f;

        public LevelData() { }

        public LevelData(float bestTime, bool unlocked, float currentTime)
        {
            this.bestTime = bestTime;
            this.unlocked = unlocked;
            this.currentTime = currentTime;
        }
    }
}

