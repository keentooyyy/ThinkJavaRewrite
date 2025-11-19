using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

namespace GameCore
{
    /// <summary>
    /// Static utility to pass target scene name to loading screen
    /// </summary>
    public static class SceneLoader
    {
        private static string targetSceneName = null;

        /// <summary>
        /// Set the target scene to load after the loading screen
        /// </summary>
        public static void SetTargetScene(string sceneName)
        {
            targetSceneName = sceneName;
        }

        /// <summary>
        /// Get the target scene name (clears it after reading)
        /// </summary>
        public static string GetAndClearTargetScene()
        {
            string scene = targetSceneName;
            targetSceneName = null;
            return scene;
        }
    }
}

