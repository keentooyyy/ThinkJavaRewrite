using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameProgress
{
    /// <summary>
    /// Manages local and cloud saves as plain JSON files.
    /// - local_save.json: Local save data (levels + achievements only)
    /// - cloud_save.json: Original cloud response (backup)
    /// - GameSave_Metadata.es3: Metadata (flags, timestamps, etc.)
    /// </summary>
    public static class GameSaveManager
    {
        private const string LOCAL_SAVE_FILE = "local_save.json";
        private const string CLOUD_SAVE_FILE = "cloud_save.json";
        private const string METADATA_FILE = "GameSave_Metadata.es3";
        
        // Metadata keys
        private const string FIRST_LOGIN_FLAG_KEY = "GameSave_FirstLogin";
        private const string LAST_SYNC_TIMESTAMP_KEY = "GameSave_LastSync";

        // Events
        public static event Action OnLocalSaveChanged;
        public static event Action OnCloudSaveChanged;
        public static event Action OnSyncComplete;

        /// <summary>
        /// Check if this is the first login (no local save exists or first login flag is set)
        /// </summary>
        public static bool IsFirstLogin()
        {
            // Check if first login flag exists in metadata
            if (!ES3.KeyExists(FIRST_LOGIN_FLAG_KEY, METADATA_FILE))
            {
                // Check if local save file exists
                string filePath = Path.Combine(Application.persistentDataPath, LOCAL_SAVE_FILE);
                bool hasLocalSave = File.Exists(filePath);
                
                // If no local save, it's first login
                if (!hasLocalSave)
                {
                    return true;
                }
            }
            
            // If flag exists and is true, it's first login
            return ES3.Load<bool>(FIRST_LOGIN_FLAG_KEY, METADATA_FILE, true);
        }

        /// <summary>
        /// Mark that first login has been completed
        /// </summary>
        public static void MarkFirstLoginComplete()
        {
            ES3.Save(FIRST_LOGIN_FLAG_KEY, false, METADATA_FILE);
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
        /// Sync on login/connect: Push local to cloud, then pull cloud to local
        /// This makes cloud authoritative after upload
        /// </summary>
        public static void SyncOnLogin()
        {
            Debug.Log("Syncing on login...");

            // Step 1: Push local changes to cloud
            var localData = LoadLocal();
            localData.UpdateTimestamp();
            SaveCloud(localData);
            Debug.Log("Pushed local changes to cloud");

            // Step 2: Just copy cloud to local - that's it!
            CopyCloudToLocal();
            Debug.Log("Copied cloud to local");

            // Update sync timestamp
            var cloudData = LoadCloud();
            SetLastSyncTimestamp(cloudData.lastModifiedTimestamp);
            OnSyncComplete?.Invoke();
        }

        /// <summary>
        /// Check if local has changes since last sync
        /// </summary>
        public static bool HasLocalChanges()
        {
            var localData = LoadLocal();
            long lastSync = GetLastSyncTimestamp();
            
            // If local was modified after last sync, there are changes
            return localData.lastModifiedTimestamp > lastSync;
        }

        /// <summary>
        /// Load local save data from JSON file - ONLY levels and achievements
        /// Uses ES3 to parse the JSON
        /// </summary>
        public static GameSaveData LoadLocal()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, LOCAL_SAVE_FILE);
                if (!File.Exists(filePath))
                {
                    return new GameSaveData();
                }
                
                string json = File.ReadAllText(filePath);
                if (string.IsNullOrEmpty(json))
                {
                    return new GameSaveData();
                }
                
                // Use ES3 to parse the JSON
                const string TEMP_FILE = "temp_load.json";
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
                
                // Clean up temp file
                if (ES3.FileExists(TEMP_FILE))
                    ES3.DeleteFile(TEMP_FILE);
                
                // Load timestamp from METADATA (not from save file!)
                if (ES3.KeyExists(LAST_SYNC_TIMESTAMP_KEY, METADATA_FILE))
                {
                    data.lastModifiedTimestamp = ES3.Load<long>(LAST_SYNC_TIMESTAMP_KEY, METADATA_FILE, 0);
                }
                
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load local save: {e.Message}\nStack trace: {e.StackTrace}");
                return new GameSaveData();
            }
        }

        /// <summary>
        /// Save data to local storage as plain JSON - ONLY levels and achievements (NO timestamp!)
        /// Uses ES3 to serialize to plain JSON format
        /// </summary>
        public static void SaveLocal(GameSaveData data)
        {
            if (data == null)
            {
                Debug.LogError("Cannot save null GameSaveData");
                return;
            }

            try
            {
                // Use ES3 to save - it will create plain JSON
                const string TEMP_FILE = "temp_local.json";
                
                // Save only levels and achievements
                var es3File = new ES3File(TEMP_FILE, false);
                es3File.Save("levels", data.levels);
                es3File.Save("achievements", data.achievements);
                es3File.Sync();
                
                // Get the raw JSON string
                string json = es3File.LoadRawString();
                
                // Save to local_save.json
                string filePath = Path.Combine(Application.persistentDataPath, LOCAL_SAVE_FILE);
                File.WriteAllText(filePath, json);
                
                // Clean up temp file
                if (ES3.FileExists(TEMP_FILE))
                    ES3.DeleteFile(TEMP_FILE);
                
                // Update timestamp in metadata only
                data.UpdateTimestamp();
                ES3.Save(LAST_SYNC_TIMESTAMP_KEY, data.lastModifiedTimestamp, METADATA_FILE);
                
                OnLocalSaveChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save local data: {e.Message}");
            }
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
        /// Sync local save to cloud (cloud becomes a copy of local)
        /// Useful for testing or manual sync
        /// </summary>
        public static void SyncLocalToCloud()
        {
            var localData = LoadLocal();
            SaveCloud(localData);
            SetLastSyncTimestamp(localData.lastModifiedTimestamp);
            Debug.Log("Synced local save to cloud");
        }

        /// <summary>
        /// Copy cloud_save.json to local_save.json - verifies file has content before copying
        /// </summary>
        public static void CopyCloudToLocal()
        {
            string cloudPath = Path.Combine(Application.persistentDataPath, CLOUD_SAVE_FILE);
            string localPath = Path.Combine(Application.persistentDataPath, LOCAL_SAVE_FILE);
            
            if (!File.Exists(cloudPath))
            {
                Debug.LogError($"Cloud save file not found: {cloudPath}");
                return;
            }
            
            // Verify file has content before copying
            var fileInfo = new FileInfo(cloudPath);
            if (fileInfo.Length == 0)
            {
                Debug.LogError($"Cannot copy {CLOUD_SAVE_FILE} - file is empty!");
                return;
            }
            
            File.Copy(cloudPath, localPath, true);
            Debug.Log($"Copied to {LOCAL_SAVE_FILE}");
            OnLocalSaveChanged?.Invoke();
        }


        /// <summary>
        /// Save raw JSON from API following the flow:
        /// 1. Save to cloud_save.json
        /// 2. Verify it matches original response (1:1 comparison)
        /// 3. Copy to local_save.json
        /// 4. Complete
        /// </summary>
        public static bool SaveCloudJsonToLocal(string originalJson)
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
                
                // Copy to local_save.json
                string localPath = Path.Combine(Application.persistentDataPath, LOCAL_SAVE_FILE);
                File.WriteAllText(localPath, savedJson);
                
                // Verify copy
                if (!File.Exists(localPath))
                {
                    Debug.LogError($"Failed to create {LOCAL_SAVE_FILE}!");
                    return false;
                }
                
                string copiedContent = File.ReadAllText(localPath);
                if (copiedContent != savedJson)
                {
                    Debug.LogError($"Copy failed! Content mismatch.");
                    return false;
                }
                
                OnLocalSaveChanged?.Invoke();
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
        /// Get local save as JSON string for API upload
        /// Reads from local_save.json
        /// </summary>
        public static string GetLocalSaveAsJson()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, LOCAL_SAVE_FILE);
                if (!File.Exists(filePath))
                {
                    return "{}";
                }
                
                // Read JSON file
                string json = File.ReadAllText(filePath);
                
                // If empty, return empty JSON object
                if (string.IsNullOrEmpty(json))
                {
                    return "{}";
                }
                
                return json;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get local save as JSON: {e.Message}");
                return "{}";
            }
        }

        /// <summary>
        /// Clear all saves (for testing/reset)
        /// </summary>
        public static void ClearAllSaves()
        {
            // Delete JSON save files
            string localPath = Path.Combine(Application.persistentDataPath, LOCAL_SAVE_FILE);
            if (File.Exists(localPath))
                File.Delete(localPath);
            
            string cloudPath = Path.Combine(Application.persistentDataPath, CLOUD_SAVE_FILE);
            if (File.Exists(cloudPath))
                File.Delete(cloudPath);
            
            // Clear metadata
            if (ES3.KeyExists(FIRST_LOGIN_FLAG_KEY, METADATA_FILE))
                ES3.DeleteKey(FIRST_LOGIN_FLAG_KEY, METADATA_FILE);
            if (ES3.KeyExists(LAST_SYNC_TIMESTAMP_KEY, METADATA_FILE))
                ES3.DeleteKey(LAST_SYNC_TIMESTAMP_KEY, METADATA_FILE);
            
            OnLocalSaveChanged?.Invoke();
            OnCloudSaveChanged?.Invoke();
        }

        /// <summary>
        /// Reset first login flag (for testing)
        /// </summary>
        public static void ResetFirstLoginFlag()
        {
            if (ES3.KeyExists(FIRST_LOGIN_FLAG_KEY, METADATA_FILE))
                ES3.DeleteKey(FIRST_LOGIN_FLAG_KEY, METADATA_FILE);
        }
    }
}

