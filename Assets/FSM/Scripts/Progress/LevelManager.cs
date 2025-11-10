using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameProgress
{
    /// <summary>
    /// Runtime manager for level progress. Works with local save as source of truth.
    /// </summary>
    public static class LevelManager
    {
        // Events
        public static event Action<string> OnLevelUnlocked;
        public static event Action<string, float> OnBestTimeUpdated;

        /// <summary>
        /// Get all levels from local save
        /// </summary>
        public static Dictionary<string, LevelData> GetAllLevels()
        {
            var saveData = GameSaveManager.LoadLocal();
            return saveData.levels ?? new Dictionary<string, LevelData>();
        }

        /// <summary>
        /// Get specific level data
        /// </summary>
        public static LevelData GetLevel(string levelId)
        {
            var levels = GetAllLevels();
            if (levels.ContainsKey(levelId))
            {
                return levels[levelId];
            }
            return new LevelData(0f, false, 0f);
        }

        /// <summary>
        /// Check if level is unlocked
        /// </summary>
        public static bool IsUnlocked(string levelId)
        {
            var level = GetLevel(levelId);
            return level.unlocked;
        }

        /// <summary>
        /// Unlock a level (updates local save with timestamp)
        /// </summary>
        public static void UnlockLevel(string levelId)
        {
            var saveData = GameSaveManager.LoadLocal();

            if (!saveData.levels.ContainsKey(levelId))
            {
                saveData.levels[levelId] = new LevelData(0f, true, 0f);
            }
            else
            {
                var level = saveData.levels[levelId];
                if (!level.unlocked)
                {
                    level.unlocked = true;
                    // Save updates timestamp automatically
                    GameSaveManager.SaveLocal(saveData);
                    OnLevelUnlocked?.Invoke(levelId);
                    Debug.Log($"Level unlocked: {levelId}");
                    return;
                }
            }

            // Save if new level was created
            GameSaveManager.SaveLocal(saveData);
        }

        /// <summary>
        /// Update level completion time (updates bestTime if better) - updates local save
        /// </summary>
        public static void UpdateLevelTime(string levelId, float completionTime)
        {
            var saveData = GameSaveManager.LoadLocal();

            if (!saveData.levels.ContainsKey(levelId))
            {
                saveData.levels[levelId] = new LevelData(completionTime, true, completionTime);
            }
            else
            {
                var level = saveData.levels[levelId];
                level.currentTime = completionTime;

                // Update best time if this is better (lower time is better, or if no best time exists)
                if (level.bestTime <= 0f || completionTime < level.bestTime)
                {
                    level.bestTime = completionTime;
                    OnBestTimeUpdated?.Invoke(levelId, completionTime);
                }
            }

            // Save updates timestamp automatically
            GameSaveManager.SaveLocal(saveData);
        }

        /// <summary>
        /// Get best time for a level
        /// </summary>
        public static float GetBestTime(string levelId)
        {
            var level = GetLevel(levelId);
            return level.bestTime;
        }

        /// <summary>
        /// Get current time for a level
        /// </summary>
        public static float GetCurrentTime(string levelId)
        {
            var level = GetLevel(levelId);
            return level.currentTime;
        }

        /// <summary>
        /// Lock a level (for testing/reset) - updates local save
        /// </summary>
        public static void LockLevel(string levelId)
        {
            var saveData = GameSaveManager.LoadLocal();
            if (saveData.levels.ContainsKey(levelId))
            {
                saveData.levels[levelId].unlocked = false;
                GameSaveManager.SaveLocal(saveData);
            }
        }
    }
}

