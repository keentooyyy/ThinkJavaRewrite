using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameProgress;
using GameUI;
using GameEvents;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/UI")]
    [Description("Populate Levels UI by instantiating level button prefabs into Slide Content. Configures buttons based on unlocked status (shows/hides lock icon, enables/disables button, shows/hides text group).")]
    public class PopulateLevelsUIAction : ActionTask
    {
        [UnityEngine.Header("Container")]
        [RequiredField]
        [Tooltip("Slide Content Transform where level buttons will be instantiated")]
        public BBParameter<Transform> slideContentContainer;

        [UnityEngine.Header("Prefab")]
        [RequiredField]
        [Tooltip("Level button prefab to instantiate for each level")]
        public BBParameter<GameObject> levelButtonPrefab;

        [UnityEngine.Header("Prefab Structure (Child Names)")]
        [Tooltip("Name of the Button component GameObject (for enabling/disabling). Leave empty to auto-detect.")]
        public BBParameter<string> buttonChildName = "Button";

        [Tooltip("Name of the Lock Icon GameObject (shown when locked, hidden when unlocked). Leave empty to auto-detect.")]
        public BBParameter<string> lockIconChildName = "Lock Icon";

        [Tooltip("Name of the Texts Group GameObject (shown when unlocked, hidden when locked). Leave empty to auto-detect.")]
        public BBParameter<string> textsGroupChildName = "Texts Group";

        [Tooltip("Name of the Level Name Text component (TMP_Text). Leave empty to auto-detect.")]
        public BBParameter<string> levelNameTextChildName = "Level Name";

        [UnityEngine.Header("Levels Info UI")]
        [Tooltip("LevelsInfoUI component to update when level button is clicked. Leave empty to auto-detect in scene.")]
        public BBParameter<LevelsInfoUI> levelsInfoUI;

        [Tooltip("Event name to trigger when level button is clicked (e.g., 'ShowLevelInfo'). The Levels Info UI should listen to this event via UIEventListener.")]
        public BBParameter<string> levelButtonClickEvent = "ShowLevelInfo";

        [UnityEngine.Header("Formatting")]
        [Tooltip("Format string for best time display (e.g., '{0:F1}s' for 1 decimal). Default: '{0:F2}s'")]
        public BBParameter<string> timeFormat = "{0:F2}s";

        [Tooltip("Text to show when best time is 0 (not completed yet)")]
        public BBParameter<string> noTimeText = "--";

        protected override string info => "Populate Levels UI";

        protected override void OnExecute()
        {
            var container = slideContentContainer.value;
            var prefab = levelButtonPrefab.value;


            if (container == null)
            {
                Debug.LogError("PopulateLevelsUIAction: Slide Content container is null!");
                EndAction(false);
                return;
            }

            if (prefab == null)
            {
                Debug.LogError("PopulateLevelsUIAction: Level button prefab is null!");
                EndAction(false);
                return;
            }

            // Get all levels from LevelManager (uses ES3/GameSaveManager)
            var allLevels = LevelManager.GetAllLevels();
            
            // Fallback: try direct LoadCloud if LevelManager returns empty
            if (allLevels == null || allLevels.Count == 0)
            {
                var saveData = GameSaveManager.LoadCloud();
                allLevels = saveData?.levels;
            }
            
            if (allLevels == null || allLevels.Count == 0)
            {
                Debug.LogWarning("PopulateLevelsUIAction: No levels found. Make sure this runs after HandleFirstLogin syncs cloud save.");
                EndAction(false);
                return;
            }

            // Log all level IDs found
            foreach (var kvp in allLevels)
            {
            }

            // Clear existing children
            ClearContainer(container);

            // Get level order (use provided order or alphabetical)
            var levelIds = GetLevelOrder(allLevels);

            // Resolve LevelsInfoUI component (auto-detect if not provided)
            var infoUI = levelsInfoUI.value;
            if (infoUI == null)
            {
                // First try to find by GameObject name (works even if inactive)
                var levelsInfoGameObject = GameObject.Find("Levels Info UI");
                if (levelsInfoGameObject != null)
                {
                    infoUI = levelsInfoGameObject.GetComponent<LevelsInfoUI>();
                }
                
                // Fallback: search all objects including inactive
                if (infoUI == null)
                {
                    var allLevelsInfoUIs = Resources.FindObjectsOfTypeAll<LevelsInfoUI>();
                    if (allLevelsInfoUIs != null && allLevelsInfoUIs.Length > 0)
                    {
                        infoUI = allLevelsInfoUIs[0];
                    }
                }
            }

            // Instantiate and configure each level button
            int spawnedCount = 0;
            foreach (var levelId in levelIds)
            {
                if (!allLevels.ContainsKey(levelId))
                {
                    Debug.LogWarning($"PopulateLevelsUIAction: Level {levelId} not found in allLevels dictionary, skipping");
                    continue;
                }

                var levelData = allLevels[levelId];
                var instance = UnityEngine.Object.Instantiate(prefab, container);
                instance.name = $"{levelId} Button";
                
                ConfigureLevelButton(instance, levelId, levelData, infoUI);
                spawnedCount++;
            }

            EndAction(true);
        }

        private List<string> GetLevelOrder(Dictionary<string, LevelData> allLevels)
        {
            var ordered = new List<string>();
            
            // Always put Tutorial first (Intro)
            if (allLevels.ContainsKey("Tutorial"))
            {
                ordered.Add("Tutorial");
            }
            
            // Then add all Level* entries in numerical order (Level1, Level2, Level3, etc.)
            var levelKeys = allLevels.Keys
                .Where(k => k.StartsWith("Level") && k.Length > 5)
                .OrderBy(k =>
                {
                    // Extract number from "Level1", "Level2", etc.
                    var numberStr = k.Substring(5);
                    if (int.TryParse(numberStr, out int num))
                    {
                        return num;
                    }
                    return int.MaxValue; // Put invalid formats at the end
                })
                .ToList();
            
            ordered.AddRange(levelKeys);
            
            // Add any other levels that don't match the pattern (alphabetically)
            var otherKeys = allLevels.Keys
                .Where(k => k != "Tutorial" && !k.StartsWith("Level"))
                .OrderBy(k => k)
                .ToList();
            
            ordered.AddRange(otherKeys);
            
            return ordered;
        }

        private void ConfigureLevelButton(GameObject instance, string levelId, LevelData levelData, LevelsInfoUI infoUI)
        {
            // Set instance name
            instance.name = $"{levelId} Button";

            // Find child components
            var button = FindChildComponent<Button>(instance, buttonChildName.value);
            var lockIcon = FindChildGameObject(instance, lockIconChildName.value);
            var textsGroup = FindChildGameObject(instance, textsGroupChildName.value);
            var levelNameText = FindChildComponent<TMP_Text>(instance, levelNameTextChildName.value);

            // Wire up button click to trigger event for Levels Info UI
            if (button != null && levelData.unlocked)
            {
                // Create a proper closure by copying levelId and levelData to local variables
                // This ensures each button captures its own values, not the loop variable
                string buttonLevelId = levelId;
                LevelData buttonLevelData = levelData;
                
                button.onClick.AddListener(() =>
                {
                    // Set the clicked level ID so LevelsInfoUI can read it when event fires
                    LevelsInfoUI.SetClickedLevelId(buttonLevelId);
                    
                    // Find LevelsInfoUI dynamically (in case it wasn't found during OnExecute or was created later)
                    var levelsInfoUI = infoUI;
                    if (levelsInfoUI == null)
                    {
                        // Try to find by GameObject name first (works even if inactive)
                        var levelsInfoGameObject = GameObject.Find("Levels Info UI");
                        if (levelsInfoGameObject != null)
                        {
                            levelsInfoUI = levelsInfoGameObject.GetComponent<LevelsInfoUI>();
                        }
                        
                        // Fallback: search all objects including inactive
                        if (levelsInfoUI == null)
                        {
                            var allLevelsInfoUIs = Resources.FindObjectsOfTypeAll<LevelsInfoUI>();
                            if (allLevelsInfoUIs != null && allLevelsInfoUIs.Length > 0)
                            {
                                levelsInfoUI = allLevelsInfoUIs[0];
                            }
                        }
                    }
                    
                    // Update directly with the level data to avoid any timing issues
                    if (levelsInfoUI != null)
                    {
                        levelsInfoUI.UpdateLevelInfo(buttonLevelId, buttonLevelData);
                    }
                    
                    // Trigger event to show Levels Info UI (UIEventListener will handle showing)
                    string eventName = levelButtonClickEvent.value;
                    if (!string.IsNullOrEmpty(eventName))
                    {
                        UIEventManager.Trigger(eventName);
                    }
                });
            }

            // Get all TMP_Text components in Texts Group (for cases with multiple text components)
            TMP_Text[] textsGroupTexts = null;
            if (textsGroup != null)
            {
                textsGroupTexts = textsGroup.GetComponentsInChildren<TMP_Text>(true);
            }

            // Configure based on unlocked status
            if (levelData.unlocked)
            {
                // Unlocked: Enable button, hide lock icon, show text group
                if (button != null)
                {
                    button.interactable = true;
                }

                if (lockIcon != null)
                {
                    lockIcon.SetActive(false);
                }

                if (textsGroup != null)
                {
                    textsGroup.SetActive(true);
                }

                // Set level name (uppercase for consistent shadow text effect)
                string formattedLevelName = FormatLevelName(levelId);

                if (levelNameText != null)
                {
                    levelNameText.text = formattedLevelName;
                }

                if (textsGroupTexts != null && textsGroupTexts.Length > 0)
                {
                    foreach (var text in textsGroupTexts)
                    {
                        if (text != null)
                        {
                            text.text = formattedLevelName;
                        }
                    }
                }
            }
            else
            {
                // Locked: Disable button, show lock icon, hide text group
                if (button != null)
                {
                    button.interactable = false;
                }

                if (lockIcon != null)
                {
                    lockIcon.SetActive(true);
                }

                if (textsGroup != null)
                {
                    textsGroup.SetActive(false);
                }

                // Clear text fields (optional, for locked levels)
                if (levelNameText != null)
                {
                    levelNameText.text = "";
                }

                // Clear Texts Group text components
                if (textsGroupTexts != null)
                {
                    foreach (var text in textsGroupTexts)
                    {
                        if (text != null)
                        {
                            text.text = "";
                        }
                    }
                }
            }
        }

        private string FormatLevelName(string levelId)
        {
            string formatted = levelId;

            // Special case: Tutorial → Intro
            if (levelId.Equals("Tutorial", System.StringComparison.OrdinalIgnoreCase))
            {
                formatted = "Intro";
            }
            else if (levelId.StartsWith("Level"))
            {
                // Convert "Level1" to just "1", "Level2" to "2", etc.
                var number = levelId.Substring(5);
                if (int.TryParse(number, out _))
                {
                    formatted = number; // Just return the number, e.g., "1", "2", "3"
                }
            }

            return formatted.ToUpperInvariant();
        }

        private T FindChildComponent<T>(GameObject parent, string childName) where T : Component
        {
            if (string.IsNullOrEmpty(childName))
            {
                // Auto-detect: find first component of type T
                return parent.GetComponentInChildren<T>(true);
            }

            // Find by name
            var child = FindChildGameObject(parent, childName);
            if (child != null)
            {
                return child.GetComponent<T>();
            }

            return null;
        }

        private GameObject FindChildGameObject(GameObject parent, string childName)
        {
            if (string.IsNullOrEmpty(childName))
            {
                return null;
            }

            // Search in children (including inactive)
            foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
            {
                if (child.gameObject.name == childName || 
                    child.gameObject.name.Contains(childName))
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        private void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                var child = container.GetChild(i);
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
        }
    }
}

