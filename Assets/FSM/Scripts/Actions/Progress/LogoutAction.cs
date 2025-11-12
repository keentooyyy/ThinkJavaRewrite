using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Login")]
    [Description("Logout and clear all save data (credentials, profile cache, cloud save). Optionally clears profile UI and loads a scene.")]
    public class LogoutAction : ActionTask
    {
        [UnityEngine.Header("Profile UI Clearing (Optional)")]
        [Tooltip("Optional: Clear profile picture image")]
        public BBParameter<Image> profileImage;
        
        [Tooltip("Optional: Clear student ID text")]
        public BBParameter<TMP_Text> studentIdText;
        
        [Tooltip("Optional: Clear student name text")]
        public BBParameter<TMP_Text> studentNameText;
        
        [Tooltip("Optional: Clear section text")]
        public BBParameter<TMP_Text> sectionText;
        
        [Tooltip("Optional: Clear bio text")]
        public BBParameter<TMP_Text> bioText;

        [UnityEngine.Header("Scene Management (Optional)")]
        [Tooltip("Optional: Scene name to load after logout (e.g., 'MainMenu'). Leave empty to stay in current scene.")]
        public BBParameter<string> loadSceneName;

        private const string SAVE_FILE = "SaveFile.es3";
        private const string LOGIN_DATA_FILE = "LoginData.es3";
        private const string CLOUD_SAVE_FILE = "cloud_save.json";
        private const string PROFILE_PIC_CACHE_PREFIX = "ProfilePicCache_";

        protected override string info
        {
            get
            {
                if (!string.IsNullOrEmpty(loadSceneName.value))
                    return $"Logout → Load Scene: {loadSceneName.value}";
                return "Logout";
            }
        }

        protected override void OnExecute()
        {
            // Clear credentials (LoginData.es3)
            LoginManager.Logout();

            // Delete LoginData.es3 file completely
            ClearLoginDataFile();

            // Clear profile picture cache from SaveFile.es3
            ClearProfilePictureCache();

            // Clear cloud save file
            ClearCloudSave();

            // Clear profile UI elements if provided
            ClearProfileUI();

            // Load scene if specified
            if (!string.IsNullOrEmpty(loadSceneName.value))
            {
                SceneManager.LoadScene(loadSceneName.value);
            }

            EndAction(true);
        }

        private void ClearLoginDataFile()
        {
            try
            {
                if (ES3.FileExists(LOGIN_DATA_FILE))
                {
                    ES3.DeleteFile(LOGIN_DATA_FILE);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"LogoutAction: Failed to delete login data file: {e.Message}");
            }
        }

        private void ClearProfilePictureCache()
        {
            try
            {
                if (ES3.FileExists(SAVE_FILE))
                {
                    // Get all keys from SaveFile.es3
                    var es3File = new ES3File(SAVE_FILE);
                    string[] allKeys = es3File.GetKeys();
                    
                    // Delete all profile picture cache keys
                    foreach (string key in allKeys)
                    {
                        if (key.StartsWith(PROFILE_PIC_CACHE_PREFIX))
                        {
                            ES3.DeleteKey(key, SAVE_FILE);
                        }
                    }
                    
                    // Reload file to check if any keys remain
                    es3File = new ES3File(SAVE_FILE);
                    string[] remainingKeys = es3File.GetKeys();
                    
                    // If no keys remain, delete the entire file
                    if (remainingKeys.Length == 0)
                    {
                        ES3.DeleteFile(SAVE_FILE);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"LogoutAction: Failed to clear profile picture cache: {e.Message}");
            }
        }

        private void ClearCloudSave()
        {
            try
            {
                string cloudSavePath = Path.Combine(Application.persistentDataPath, CLOUD_SAVE_FILE);
                if (File.Exists(cloudSavePath))
                {
                    File.Delete(cloudSavePath);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"LogoutAction: Failed to delete cloud save file: {e.Message}");
            }
        }

        private void ClearProfileUI()
        {
            // Clear profile picture
            if (profileImage.value != null)
            {
                profileImage.value.sprite = null;
                profileImage.value.enabled = false;
            }

            // Clear text fields
            if (studentIdText.value != null)
                studentIdText.value.text = "";

            if (studentNameText.value != null)
                studentNameText.value.text = "";

            if (sectionText.value != null)
                sectionText.value.text = "";

            if (bioText.value != null)
                bioText.value.text = "";
        }
    }
}

