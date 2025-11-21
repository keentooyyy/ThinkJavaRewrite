using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameProgress;
using GameCore;
using System.Collections.Generic;
using System.Linq;

namespace NodeCanvas.Tasks.Actions
{
    /// <summary>
    /// Handles the Play button logic:
    /// - If Tutorial is already played (has bestTime/currentTime), load the last unlocked level
    /// - Otherwise, load Tutorial
    /// Uses LoadingScene to transition
    /// </summary>
    [Category("✫ Custom/UI Events")]
    [Description("Handle Play button - loads Tutorial or last unlocked level based on save data")]
    public class HandlePlayButtonAction : ActionTask
    {
        [Tooltip("Scene name for the tutorial (default: TutorialScene)")]
        public BBParameter<string> tutorialSceneName = "TutorialScene";

        [Tooltip("Scene name for the loading screen (default: LoadingScene)")]
        public BBParameter<string> loadingSceneName = "LoadingScene";

        protected override string info
        {
            get { return "Handle Play Button"; }
        }

        protected override void OnExecute()
        {
            string targetScene = DetermineTargetScene();
            
            if (string.IsNullOrEmpty(targetScene))
            {
                Debug.LogWarning("HandlePlayButtonAction: Could not determine target scene, defaulting to Tutorial");
                targetScene = tutorialSceneName.value;
            }

            Debug.Log($"HandlePlayButtonAction: Loading scene '{targetScene}'");
            
            // Set the target scene for the loading screen
            SceneLoader.SetTargetScene(targetScene);
            
            // Load the loading screen scene
            SceneManager.LoadScene(loadingSceneName.value);
            
            EndAction(true);
        }

        private string DetermineTargetScene()
        {
            // Load cloud save data
            var saveData = GameSaveManager.LoadCloud();
            
            if (saveData == null || saveData.levels == null)
            {
                Debug.LogWarning("HandlePlayButtonAction: No save data found, loading Tutorial");
                return tutorialSceneName.value;
            }

            // Check if Tutorial has been played (has bestTime or currentTime > 0)
            bool tutorialPlayed = false;
            if (saveData.levels.ContainsKey("Tutorial"))
            {
                var tutorialData = saveData.levels["Tutorial"];
                tutorialPlayed = tutorialData.bestTime > 0f || tutorialData.currentTime > 0f;
            }

            // If Tutorial hasn't been played, load Tutorial
            if (!tutorialPlayed)
            {
                Debug.Log("HandlePlayButtonAction: Tutorial not played yet, loading Tutorial");
                return tutorialSceneName.value;
            }

            // Find the last unlocked level
            string lastUnlockedLevel = FindLastUnlockedLevel(saveData.levels);
            
            if (string.IsNullOrEmpty(lastUnlockedLevel))
            {
                Debug.LogWarning("HandlePlayButtonAction: No unlocked levels found, loading Tutorial");
                return tutorialSceneName.value;
            }

            // Convert level ID to scene name
            string sceneName = ConvertLevelIdToSceneName(lastUnlockedLevel);
            Debug.Log($"HandlePlayButtonAction: Tutorial already played, loading last unlocked level: {lastUnlockedLevel} ({sceneName})");
            
            return sceneName;
        }

        private string FindLastUnlockedLevel(Dictionary<string, LevelData> levels)
        {
            if (levels == null || levels.Count == 0)
                return null;

            // Get all unlocked levels
            var unlockedLevels = levels
                .Where(kvp => kvp.Value.unlocked)
                .ToList();

            if (unlockedLevels.Count == 0)
                return null;

            // Separate Tutorial and numbered levels
            var tutorialLevel = unlockedLevels.FirstOrDefault(kvp => kvp.Key.Equals("Tutorial", System.StringComparison.OrdinalIgnoreCase));
            var numberedLevels = unlockedLevels
                .Where(kvp => kvp.Key.StartsWith("Level", System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(kvp =>
                {
                    // Extract number from "Level1", "Level2", etc.
                    string key = kvp.Key;
                    if (key.Length > 5)
                    {
                        string numberStr = key.Substring(5);
                        if (int.TryParse(numberStr, out int num))
                        {
                            return num;
                        }
                    }
                    return -1; // Invalid format, put at beginning
                })
                .Where(kvp =>
                {
                    // Filter out invalid formats
                    string key = kvp.Key;
                    if (key.Length > 5)
                    {
                        string numberStr = key.Substring(5);
                        return int.TryParse(numberStr, out _);
                    }
                    return false;
                })
                .ToList();

            // If we have numbered levels, return the highest one
            if (numberedLevels.Count > 0)
            {
                return numberedLevels.Last().Key;
            }

            // Otherwise, if Tutorial is unlocked, return it
            if (tutorialLevel.Key != null)
            {
                return tutorialLevel.Key;
            }

            // Fallback: return the first unlocked level (alphabetically)
            return unlockedLevels.OrderBy(kvp => kvp.Key).First().Key;
        }

        private string ConvertLevelIdToSceneName(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
                return null;

            // Special case: Tutorial → TutorialScene
            if (levelId.Equals("Tutorial", System.StringComparison.OrdinalIgnoreCase))
            {
                return "TutorialScene";
            }

            // Level1 → Level1Scene, Level2 → Level2Scene, etc.
            if (levelId.StartsWith("Level", System.StringComparison.OrdinalIgnoreCase))
            {
                return $"{levelId}Scene";
            }

            // Fallback: try appending "Scene" to the level ID
            return $"{levelId}Scene";
        }
    }
}

