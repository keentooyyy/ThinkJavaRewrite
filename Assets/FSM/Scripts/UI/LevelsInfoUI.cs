using UnityEngine;
using TMPro;
using GameProgress;
using GameScoring;
using GameEvents;

namespace GameUI
{
    /// <summary>
    /// Reusable component for Levels Info UI that updates level text and stars based on bestTime from cloud_save.json
    /// Listens to UI events to update and show itself
    /// </summary>
    public class LevelsInfoUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Level name text component. Leave empty to auto-detect by name 'Level Text'")]
        [SerializeField] private TMP_Text levelText;

        [Tooltip("Score text component. Leave empty to auto-detect by name 'Score'")]
        [SerializeField] private TMP_Text scoreText;

        [Tooltip("Stars container GameObject. Leave empty to auto-detect by name 'Stars'")]
        [SerializeField] private Transform starsContainer;

        [Header("Events")]
        [Tooltip("Event name to listen for to update and show this UI (e.g., 'ShowLevelInfo')")]
        [SerializeField] private string showEventName = "ShowLevelInfo";

        [Header("Score Display")]
        [Tooltip("Format string for score display (e.g., '000' for 3 digits with leading zeros). Default: '000'")]
        [SerializeField] private string scoreFormat = "000";

        [Header("Star Display")]
        [Tooltip("Alpha value for inactive (not earned) stars")]
        [SerializeField] private float inactiveStarAlpha = 0.25f;

        private Transform[] starTransforms;
        
        // Static storage for the levelId that was clicked (set by button, read by this component)
        private static string lastClickedLevelId = null;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            // Subscribe to the show event
            if (!string.IsNullOrEmpty(showEventName))
            {
                UIEventManager.Subscribe(showEventName, OnShowEvent);
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from the show event
            if (!string.IsNullOrEmpty(showEventName))
            {
                UIEventManager.Unsubscribe(showEventName, OnShowEvent);
            }
        }

        private void OnShowEvent()
        {
            // Update with the last clicked level
            // Note: The button click handler also calls UpdateLevelInfo directly,
            // but we keep this as a fallback in case the event fires from elsewhere
            if (!string.IsNullOrEmpty(lastClickedLevelId))
            {
                UpdateLevelInfo(lastClickedLevelId);
            }
        }

        /// <summary>
        /// Set the level ID that was clicked (called by button click handler)
        /// </summary>
        public static void SetClickedLevelId(string levelId)
        {
            lastClickedLevelId = levelId;
        }

        private void ResolveReferences()
        {
            // Auto-detect level text if not assigned
            if (levelText == null)
            {
                levelText = transform.Find("Level Text")?.GetComponent<TMP_Text>();
                if (levelText == null)
                {
                    // Try to find it by searching all children
                    var allTexts = GetComponentsInChildren<TMP_Text>(true);
                    foreach (var text in allTexts)
                    {
                        if (text.gameObject.name == "Level Text")
                        {
                            levelText = text;
                            break;
                        }
                    }
                }
            }

            // Auto-detect score text if not assigned
            if (scoreText == null)
            {
                var scoreGameObject = transform.Find("Score");
                if (scoreGameObject != null)
                {
                    // First try to find TMP_Text directly on Score GameObject
                    scoreText = scoreGameObject.GetComponent<TMP_Text>();
                    if (scoreText == null)
                    {
                        // Try to find TMP_Text in Score GameObject's children (ScoreCounterDOTween might have it)
                        scoreText = scoreGameObject.GetComponentInChildren<TMP_Text>(true);
                    }
                }
            }
            
            // Also try to find "Label" GameObject's text (if it exists and needs updating)
            // This is handled separately in UpdateScore method

            // Auto-detect stars container if not assigned
            if (starsContainer == null)
            {
                starsContainer = transform.Find("Stars");
            }

            // Get star transforms from container
            // Sort by sibling index to ensure correct left-to-right order
            if (starsContainer != null)
            {
                starTransforms = new Transform[starsContainer.childCount];
                for (int i = 0; i < starsContainer.childCount; i++)
                {
                    starTransforms[i] = starsContainer.GetChild(i);
                }
                
                // Sort by sibling index to ensure consistent order (left-to-right)
                System.Array.Sort(starTransforms, (a, b) => a.GetSiblingIndex().CompareTo(b.GetSiblingIndex()));
            }
        }

        /// <summary>
        /// Update the Levels Info UI with level data from cloud save
        /// </summary>
        public void UpdateLevelInfo(string levelId)
        {
            // Get level data from cloud save
            var allLevels = LevelManager.GetAllLevels();
            if (allLevels == null || allLevels.Count == 0)
            {
                var saveData = GameSaveManager.LoadCloud();
                allLevels = saveData?.levels;
            }

            if (allLevels == null || !allLevels.ContainsKey(levelId))
            {
                Debug.LogWarning($"LevelsInfoUI: Level '{levelId}' not found in save data");
                return;
            }

            var levelData = allLevels[levelId];
            UpdateLevelInfo(levelId, levelData);
        }

        /// <summary>
        /// Update the Levels Info UI with specific level data
        /// </summary>
        public void UpdateLevelInfo(string levelId, LevelData levelData)
        {
            ResolveReferences();

            // Update level text
            if (levelText != null)
            {
                string formattedLevelName = FormatLevelName(levelId);
                levelText.text = formattedLevelName;
            }

            // Calculate and display score based on bestTime
            int score = 0;
            if (levelData.bestTime > 0f)
            {
                // Use the existing LevelScoreCalculator to get score from bestTime
                score = LevelScoreCalculator.CalcLevelScore(levelData.bestTime);
            }

            UpdateScore(score);

            // Calculate and display stars based on bestTime
            int starsEarned = 0;
            if (levelData.bestTime > 0f)
            {
                // Use the existing LevelScoreCalculator to get stars from bestTime
                starsEarned = LevelScoreCalculator.CalcLevelStars(levelData.bestTime);
            }

            UpdateStars(starsEarned);
        }

        private string FormatLevelName(string levelId)
        {
            string formatted = levelId;

            // Special case: Tutorial → Intro (keep as is)
            if (levelId.Equals("Tutorial", System.StringComparison.OrdinalIgnoreCase))
            {
                formatted = "Intro";
            }
            else if (levelId.StartsWith("Level"))
            {
                // Convert "Level1" to "Level 1", "Level2" to "Level 2", etc.
                var number = levelId.Substring(5);
                if (int.TryParse(number, out _))
                {
                    formatted = $"Level {number}";
                }
            }

            return formatted;
        }

        private void UpdateScore(int score)
        {
            string scoreString = score.ToString(scoreFormat);
            
            // Update the main score text only (do not touch "Label" GameObject)
            if (scoreText != null)
            {
                scoreText.text = scoreString;
            }
        }

        private void UpdateStars(int starsEarned)
        {
            if (starTransforms == null || starTransforms.Length == 0)
            {
                return;
            }

            // Need at least 3 stars for this display pattern
            if (starTransforms.Length < 3)
            {
                return;
            }

            // Star display pattern:
            // - 3 stars (perfect/100 score): Show middle star only (index 1)
            // - 2 stars (70 or 40 score): Show left and right stars (index 0 and 2)
            // - 1 star (10 score): Show left star only (index 0)
            // - 0 stars: Show none (all dimmed)
            
            bool showLeft = starsEarned >= 1;      // Left star (index 0): 1+ stars
            bool showMiddle = starsEarned >= 3;    // Middle star (index 1): 3 stars (perfect)
            bool showRight = starsEarned >= 2;    // Right star (index 2): 2+ stars
            
            for (int i = 0; i < starTransforms.Length; i++)
            {
                var star = starTransforms[i];
                if (star == null) continue;

                // Get or add CanvasGroup for alpha control
                var canvasGroup = star.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = star.gameObject.AddComponent<CanvasGroup>();
                }

                // Determine if this star should be shown based on position and stars earned
                bool shouldShow = false;
                if (i == 0)
                {
                    // Left star: show for 1+ stars
                    shouldShow = showLeft;
                }
                else if (i == 1)
                {
                    // Middle star: show for 3 stars (perfect score)
                    shouldShow = showMiddle;
                }
                else if (i == 2)
                {
                    // Right star: show for 2+ stars
                    shouldShow = showRight;
                }
                
                canvasGroup.alpha = shouldShow ? 1f : inactiveStarAlpha;
            }
        }
    }
}

