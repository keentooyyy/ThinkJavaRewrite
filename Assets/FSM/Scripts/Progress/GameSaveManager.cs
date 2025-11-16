using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameProgress
{
    /// <summary>
    /// Manages cloud save as plain JSON file.
    /// - cloud_save.json: Cloud save data (levels + achievements + timestamp)
    /// - GameSave_Metadata.es3: Metadata (timestamps, etc.)
    /// </summary>
    public static class GameSaveManager
    {
        private const string CLOUD_SAVE_FILE = "cloud_save.json";
        private const string METADATA_FILE = "GameSave_Metadata.es3";
        
        // Metadata keys
        private const string LAST_SYNC_TIMESTAMP_KEY = "GameSave_LastSync";

        // Events
        public static event Action OnCloudSaveChanged;

        /// <summary>
        /// Compare cloud API response JSON with cloud_save.json content
        /// Returns true if they're different
        /// </summary>
        public static bool IsCloudResponseDifferent(string apiResponseJson)
        {
            if (string.IsNullOrEmpty(apiResponseJson))
                return true; // If API response is empty but we have a save, they're different
            
            string cloudSavePath = Path.Combine(Application.persistentDataPath, CLOUD_SAVE_FILE);
            if (!File.Exists(cloudSavePath))
                return true; // No cloud save exists, so they're different
            
            try
            {
                string cloudSaveJson = File.ReadAllText(cloudSavePath);
                if (string.IsNullOrEmpty(cloudSaveJson))
                    return true; // Cloud save is empty, so they're different
                
                // Compare raw JSON strings
                return apiResponseJson != cloudSaveJson;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to compare cloud response: {e.Message}");
                return true; // On error, assume different to be safe
            }
        }
        
        /// <summary>
        /// Get cloud_save.json as JSON string for API upload
        /// </summary>
        public static string GetCloudSaveAsJson()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, CLOUD_SAVE_FILE);
                if (!File.Exists(filePath))
                {
                    return "{}";
                }
                
                string json = File.ReadAllText(filePath);
                if (string.IsNullOrEmpty(json))
                {
                    return "{}";
                }
                
                return json;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get cloud save as JSON: {e.Message}");
                return "{}";
            }
        }

        /// <summary>
        /// Get last sync timestamp
        /// </summary>
        public static long GetLastSyncTimestamp()
        {
            if (!ES3.KeyExists(LAST_SYNC_TIMESTAMP_KEY, METADATA_FILE))
                return 0;
            
            return ES3.Load<long>(LAST_SYNC_TIMESTAMP_KEY, METADATA_FILE, 0L);
        }

        /// <summary>
        /// Set last sync timestamp
        /// </summary>
        private static void SetLastSyncTimestamp(long timestamp)
        {
            ES3.Save(LAST_SYNC_TIMESTAMP_KEY, timestamp, METADATA_FILE);
        }



        /// <summary>
        /// Load cloud save data from JSON file (original API response)
        /// </summary>
        public static GameSaveData LoadCloud()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, CLOUD_SAVE_FILE);
                if (!File.Exists(filePath))
                {
                    return new GameSaveData();
                }
                
                string json = File.ReadAllText(filePath);
                if (string.IsNullOrEmpty(json))
                {
                    return new GameSaveData();
                }
                
                // Use ES3 to parse the ES3 format JSON
                const string TEMP_FILE = "temp_cloud_load.json";
                var es3File = new ES3File(TEMP_FILE, false);
                es3File.SaveRaw(System.Text.Encoding.UTF8.GetBytes(json));
                es3File.Sync();
                
                var data = new GameSaveData();
                
                // Load levels
                if (es3File.KeyExists("levels"))
                {
                    data.levels = es3File.Load<Dictionary<string, LevelData>>("levels", new Dictionary<string, LevelData>());
                }
                else
                {
                    data.levels = new Dictionary<string, LevelData>();
                }
                
                // Load achievements
                if (es3File.KeyExists("achievements"))
                {
                    data.achievements = es3File.Load<Dictionary<string, AchievementSaveData>>("achievements", new Dictionary<string, AchievementSaveData>());
                }
                else
                {
                    data.achievements = new Dictionary<string, AchievementSaveData>();
                }
                
                // Load timestamp
                if (es3File.KeyExists("lastModifiedTimestamp"))
                {
                    data.lastModifiedTimestamp = es3File.Load<long>("lastModifiedTimestamp", 0);
                }
                
                // Clean up temp file
                if (ES3.FileExists(TEMP_FILE))
                    ES3.DeleteFile(TEMP_FILE);
                
                // Ensure dictionaries are initialized
                if (data.levels == null) data.levels = new Dictionary<string, LevelData>();
                if (data.achievements == null) data.achievements = new Dictionary<string, AchievementSaveData>();
                
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load cloud save: {e.Message}");
                return new GameSaveData();
            }
        }

        /// <summary>
        /// Save data to cloud storage (original response backup)
        /// </summary>
        public static void SaveCloud(GameSaveData data)
        {
            if (data == null)
            {
                Debug.LogError("Cannot save null GameSaveData to cloud");
                return;
            }

            try
            {
                data.UpdateTimestamp();
                
                // Use ES3 to serialize
                const string TEMP_FILE = "temp_cloud.json";
                var es3File = new ES3File(TEMP_FILE, false);
                es3File.Save("levels", data.levels);
                es3File.Save("achievements", data.achievements);
                es3File.Save("lastModifiedTimestamp", data.lastModifiedTimestamp);
                es3File.Sync();
                
                // Get the JSON string
                string json = es3File.LoadRawString();
                
                // Write to JSON file
                string filePath = Path.Combine(Application.persistentDataPath, CLOUD_SAVE_FILE);
                File.WriteAllText(filePath, json);
                
                // Clean up temp file
                if (ES3.FileExists(TEMP_FILE))
                    ES3.DeleteFile(TEMP_FILE);
                
                OnCloudSaveChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save cloud data: {e.Message}");
            }
        }



        /// <summary>
        /// Save raw JSON from API to cloud_save.json
        /// </summary>
        public static bool SaveCloudJson(string originalJson)
        {
            if (string.IsNullOrEmpty(originalJson))
            {
                Debug.LogError("Cannot save empty JSON");
                return false;
            }

            try
            {
                // Save JSON to cloud_save.json
                string cloudPath = Path.Combine(Application.persistentDataPath, CLOUD_SAVE_FILE);
                File.WriteAllText(cloudPath, originalJson);
                
                // Verify cloud save
                if (!File.Exists(cloudPath) || new FileInfo(cloudPath).Length == 0)
                {
                    Debug.LogError($"Failed to save {CLOUD_SAVE_FILE}");
                    return false;
                }
                
                // Verify content matches original
                string savedJson = File.ReadAllText(cloudPath);
                if (savedJson != originalJson)
                {
                    Debug.LogError($"Saved file does not match original response!");
                    return false;
                }
                
                OnCloudSaveChanged?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save cloud JSON: {e.Message}\nStack trace: {e.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Clear all saves (for testing/reset)
        /// </summary>
        public static void ClearAllSaves()
        {
            // Delete cloud save file
            string cloudPath = Path.Combine(Application.persistentDataPath, CLOUD_SAVE_FILE);
            if (File.Exists(cloudPath))
                File.Delete(cloudPath);
            
            // Clear metadata
            if (ES3.KeyExists(LAST_SYNC_TIMESTAMP_KEY, METADATA_FILE))
                ES3.DeleteKey(LAST_SYNC_TIMESTAMP_KEY, METADATA_FILE);
            
            OnCloudSaveChanged?.Invoke();
        }

    }
}

