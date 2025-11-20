using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using UnityEngine;
using UnityEngine.SceneManagement;
using NodeCanvas.StateMachines;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Save")]
    [Description("Update level completion time (currentTime and bestTime) based on performance from LevelScoreFSM. Gets level ID from RetryScene blackboard variable. Save is handled by SaveTriggerListener listening to ShowSuccessUI.")]
    public class UpdateLevelTimeAndSaveAction : ActionTask
    {
        protected override string info => "Update Level Time";

        protected override void OnExecute()
        {
            // Get level ID from RetryScene variable
            string levelId = GetLevelIdFromRetryScene();
            if (string.IsNullOrEmpty(levelId))
            {
                Debug.LogError("UpdateLevelTimeAndSaveAction: Could not determine level ID from RetryScene variable");
                EndAction(false);
                return;
            }

            // Get time data from LevelScoreFSM blackboard
            float elapsedTime = CalculateElapsedTimeFromLevelScoreFSM();
            if (elapsedTime < 0f)
            {
                Debug.LogError("UpdateLevelTimeAndSaveAction: Could not calculate elapsed time from LevelScoreFSM blackboard");
                EndAction(false);
                return;
            }

            // Update level time (updates currentTime and bestTime if better)
            LevelManager.UpdateLevelTime(levelId, elapsedTime);

            EndAction(true);
        }

        private string GetLevelIdFromRetryScene()
        {
            // Find UIEventsFSM directly by GameObject name
            GameObject uiEventsGO = GameObject.Find("UIEvents");
            if (uiEventsGO != null)
            {
                var uiEventsFSM = uiEventsGO.GetComponent<FSMOwner>();
                if (uiEventsFSM != null && uiEventsFSM.blackboard != null)
                {
                    var retrySceneVar = uiEventsFSM.blackboard.GetVariable<string>("RetryScene");
                    if (retrySceneVar != null && !string.IsNullOrEmpty(retrySceneVar.value))
                    {
                        string sceneName = retrySceneVar.value;
                        return ConvertSceneNameToLevelId(sceneName);
                    }
                }
            }

            // Fallback: try current scene name
            string currentScene = SceneManager.GetActiveScene().name;
            return ConvertSceneNameToLevelId(currentScene);
        }

        private string ConvertSceneNameToLevelId(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return null;

            // TutorialScene → Tutorial
            if (sceneName.Equals("TutorialScene", System.StringComparison.OrdinalIgnoreCase))
            {
                return "Tutorial";
            }

            // Level1Scene → Level1, Level2Scene → Level2, etc.
            if (sceneName.StartsWith("Level", System.StringComparison.OrdinalIgnoreCase) && 
                sceneName.EndsWith("Scene", System.StringComparison.OrdinalIgnoreCase))
            {
                // Remove "Scene" suffix
                return sceneName.Substring(0, sceneName.Length - 5); // "Scene" is 5 chars
            }

            // Fallback: try to extract level ID from scene name
            return sceneName.Replace("Scene", "");
        }

        private float CalculateElapsedTimeFromLevelScoreFSM()
        {
            // Find LevelScore GameObject directly by name
            GameObject levelScoreGO = GameObject.Find("LevelScore");
            if (levelScoreGO == null)
            {
                Debug.LogError("UpdateLevelTimeAndSaveAction: Could not find LevelScore GameObject in scene");
                return -1f;
            }

            var levelScoreFSM = levelScoreGO.GetComponent<FSMOwner>();
            if (levelScoreFSM == null || levelScoreFSM.blackboard == null)
            {
                Debug.LogError("UpdateLevelTimeAndSaveAction: LevelScore GameObject does not have FSMOwner or blackboard");
                return -1f;
            }

            // Read maxTime and outRemainingSeconds from LevelScoreFSM blackboard
            var maxTimeVar = levelScoreFSM.blackboard.GetVariable<float>("maxTime");
            var remainingVar = levelScoreFSM.blackboard.GetVariable<float>("outRemainingSeconds");

            if (maxTimeVar == null)
            {
                Debug.LogError("UpdateLevelTimeAndSaveAction: maxTime variable not found in LevelScoreFSM blackboard");
                return -1f;
            }

            if (remainingVar == null)
            {
                Debug.LogError("UpdateLevelTimeAndSaveAction: outRemainingSeconds variable not found in LevelScoreFSM blackboard");
                return -1f;
            }

            float maxTime = maxTimeVar.value;
            float remaining = remainingVar.value;

            if (maxTime <= 0f)
            {
                Debug.LogError($"UpdateLevelTimeAndSaveAction: Invalid maxTime value: {maxTime}");
                return -1f;
            }

            // Save remaining time (rounded to nearest integer)
            return Mathf.Round(remaining);
        }
    }
}

