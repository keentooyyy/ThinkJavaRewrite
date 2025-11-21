using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.SceneManagement;
using GameCore;
using NodeCanvas.StateMachines;

namespace NodeCanvas.Tasks.Actions
{
    /// <summary>
    /// Handles the Next button logic on Success UI:
    /// - Gets current scene from RetryScene blackboard variable (or current scene as fallback)
    /// - Determines the next logical scene (TutorialScene → Level1Scene, Level1Scene → Level2Scene, etc.)
    /// - Uses LoadingScene to transition
    /// </summary>
    [Category("✫ Custom/UI Events")]
    [Description("Handle Next button - loads the next scene in sequence using RetryScene blackboard variable")]
    public class HandleNextButtonAction : ActionTask
    {
        [Tooltip("Scene name for the loading screen (default: LoadingScene)")]
        public BBParameter<string> loadingSceneName = "LoadingScene";

        protected override string info
        {
            get { return "Handle Next Button"; }
        }

        protected override void OnExecute()
        {
            string currentScene = GetCurrentScene();
            string nextScene = DetermineNextScene(currentScene);
            
            if (string.IsNullOrEmpty(nextScene))
            {
                Debug.LogWarning($"HandleNextButtonAction: Could not determine next scene from '{currentScene}'. No next scene available.");
                EndAction(false);
                return;
            }

            Debug.Log($"HandleNextButtonAction: Current scene '{currentScene}', loading next scene '{nextScene}'");
            
            // Set the target scene for the loading screen
            SceneLoader.SetTargetScene(nextScene);
            
            // Load the loading screen scene
            SceneManager.LoadScene(loadingSceneName.value);
            
            EndAction(true);
        }

        private string GetCurrentScene()
        {
            // Try to get from RetryScene blackboard variable (set per scene)
            GameObject uiEventsGO = GameObject.Find("UIEvents");
            if (uiEventsGO != null)
            {
                var uiEventsFSM = uiEventsGO.GetComponent<FSMOwner>();
                if (uiEventsFSM != null && uiEventsFSM.blackboard != null)
                {
                    var retrySceneVar = uiEventsFSM.blackboard.GetVariable<string>("RetryScene");
                    if (retrySceneVar != null && !string.IsNullOrEmpty(retrySceneVar.value))
                    {
                        return retrySceneVar.value;
                    }
                }
            }

            // Fallback: use current scene name
            return SceneManager.GetActiveScene().name;
        }

        private string DetermineNextScene(string currentSceneName)
        {
            if (string.IsNullOrEmpty(currentSceneName))
            {
                return null;
            }

            // TutorialScene → Level1Scene
            if (currentSceneName.Equals("TutorialScene", System.StringComparison.OrdinalIgnoreCase))
            {
                return "Level1Scene";
            }

            // Level1Scene → Level2Scene, Level2Scene → Level3Scene, etc.
            if (currentSceneName.StartsWith("Level", System.StringComparison.OrdinalIgnoreCase) && 
                currentSceneName.EndsWith("Scene", System.StringComparison.OrdinalIgnoreCase))
            {
                // Extract the number from "Level1Scene", "Level2Scene", etc.
                // Remove "Level" prefix and "Scene" suffix
                string levelPart = currentSceneName.Substring(5, currentSceneName.Length - 10); // "Level" is 5 chars, "Scene" is 5 chars
                
                if (int.TryParse(levelPart, out int levelNumber))
                {
                    int nextLevelNumber = levelNumber + 1;
                    return $"Level{nextLevelNumber}Scene";
                }
            }

            // If we can't determine the next scene, return null
            Debug.LogWarning($"HandleNextButtonAction: Could not parse scene name '{currentSceneName}' to determine next scene");
            return null;
        }
    }
}

