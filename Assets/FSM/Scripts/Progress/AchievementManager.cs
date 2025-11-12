using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameProgress
{
    /// <summary>
    /// Runtime manager for achievements. Works with cloud save as source of truth.
    /// </summary>
    public static class AchievementManager
    {
        // Events
        public static event Action<string> OnAchievementUnlocked;

        /// <summary>
        /// Get all achievements from cloud save
        /// </summary>
        public static Dictionary<string, AchievementSaveData> GetAllAchievements()
        {
            var saveData = GameSaveManager.LoadCloud();
            return saveData.achievements ?? new Dictionary<string, AchievementSaveData>();
        }

        /// <summary>
        /// Get specific achievement
        /// </summary>
        public static AchievementSaveData GetAchievement(string achievementId)
        {
            var achievements = GetAllAchievements();
            if (achievements.ContainsKey(achievementId))
            {
                return achievements[achievementId];
            }
            return null;
        }

        /// <summary>
        /// Check if achievement is unlocked
        /// </summary>
        public static bool IsUnlocked(string achievementId)
        {
            var achievement = GetAchievement(achievementId);
            return achievement != null && achievement.unlocked;
        }

        /// <summary>
        /// Unlock an achievement (updates cloud save with timestamp)
        /// </summary>
        public static void UnlockAchievement(string achievementId, string title = null, string description = null)
        {
            var saveData = GameSaveManager.LoadCloud();

            if (!saveData.achievements.ContainsKey(achievementId))
            {
                saveData.achievements[achievementId] = new AchievementSaveData(
                    title ?? achievementId,
                    description ?? "",
                    false
                );
            }

            var achievement = saveData.achievements[achievementId];
            
            // Only trigger event if it was just unlocked
            if (!achievement.unlocked)
            {
                achievement.unlocked = true;
                if (!string.IsNullOrEmpty(title)) achievement.title = title;
                if (!string.IsNullOrEmpty(description)) achievement.description = description;

                // Save updates timestamp automatically
                GameSaveManager.SaveCloud(saveData);
                OnAchievementUnlocked?.Invoke(achievementId);
                Debug.Log($"Achievement unlocked: {achievementId} - {achievement.title}");
            }
        }

        /// <summary>
        /// Lock an achievement (for testing/reset) - updates cloud save
        /// </summary>
        public static void LockAchievement(string achievementId)
        {
            var saveData = GameSaveManager.LoadCloud();
            if (saveData.achievements.ContainsKey(achievementId))
            {
                saveData.achievements[achievementId].unlocked = false;
                GameSaveManager.SaveCloud(saveData);
            }
        }

        /// <summary>
        /// Initialize achievement if it doesn't exist
        /// </summary>
        public static void InitializeAchievement(string achievementId, string title, string description)
        {
            var saveData = GameSaveManager.LoadCloud();
            if (!saveData.achievements.ContainsKey(achievementId))
            {
                saveData.achievements[achievementId] = new AchievementSaveData(title, description, false);
                GameSaveManager.SaveCloud(saveData);
            }
        }
    }
}

