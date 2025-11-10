using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameProgress
{
    /// <summary>
    /// Manages local and cloud saves separately using ES3.
    /// - local_save.es3: Local save data (1:1 copy of cloud format, used for gameplay)
    /// - cloud_save.es3: Original cloud response (backup/reference)
    /// - GameSave_Metadata.es3: Metadata (flags, timestamps, etc.)
    /// </summary>
    public static class GameSaveManager
    {
        private const string LOCAL_SAVE_FILE = "local_save.es3";
        private const string CLOUD_SAVE_FILE = "cloud_save.es3";
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
                bool hasLocalSave = ES3.FileExists(LOCAL_SAVE_FILE);
                
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
        private static void MarkFirstLoginComplete()
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
        /// Handle first login: Check if local save exists (from cloud download), otherwise create new
        /// </summary>
        public static void HandleFirstLogin()
        {
            if (!IsFirstLogin())
            {
                Debug.Log("Not first login, skipping first login sync");
                return;
            }

            Debug.Log("First login detected - checking for existing save data");

            // Check if local save exists (could be from cloud download or previous session)
            var localData = LoadLocal();
            bool hasLocalData = localData.levels.Count > 0 || localData.achievements.Count > 0;

            if (!hasLocalData)
            {
                Debug.Log("No existing save data found - creating new local save");
                // No save data, create fresh local save
                var newLocalData = new GameSaveData();
                SaveLocal(newLocalData);
            }
            else
            {
                Debug.Log($"Existing save data found: {localData.levels.Count} levels, {localData.achievements.Count} achievements");
            }

            // Mark first login as complete
            MarkFirstLoginComplete();
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

            // Step 2: Pull cloud to local (cloud is now authoritative)
            var cloudData = LoadCloud();
            SaveLocal(cloudData);
            Debug.Log("Pulled cloud data to local (cloud is authoritative)");

            // Update sync timestamp
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
        /// Load local save data (source of truth for gameplay)
        /// Loads from local_save.es3 which is 1:1 format with cloud
        /// </summary>
        public static GameSaveData LoadLocal()
        {
            try
            {
                if (!ES3.FileExists(LOCAL_SAVE_FILE))
                {
                    return new GameSaveData();
                }
                
                var es3File = new ES3File(LOCAL_SAVE_FILE, false);
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
                
                // Ensure dictionaries are initialized
                if (data.levels == null) data.levels = new Dictionary<string, LevelData>();
                if (data.achievements == null) data.achievements = new Dictionary<string, AchievementSaveData>();
                
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load local save: {e.Message}");
                return new GameSaveData();
            }
        }

        /// <summary>
        /// Save data to local storage (source of truth)
        /// Saves to local_save.es3 in exact same format as cloud (1:1 match)
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
                data.UpdateTimestamp();
                
                // Save to local_save.es3 in same format as cloud (levels, achievements, lastModifiedTimestamp only)
                var es3File = new ES3File(LOCAL_SAVE_FILE, false);
                es3File.Save("levels", data.levels);
                es3File.Save("achievements", data.achievements);
                es3File.Save("lastModifiedTimestamp", data.lastModifiedTimestamp);
                es3File.Sync();
                
                OnLocalSaveChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save local data: {e.Message}");
            }
        }

        /// <summary>
        /// Load cloud save data (original response from API)
        /// </summary>
        public static GameSaveData LoadCloud()
        {
            try
            {
                if (!ES3.FileExists(CLOUD_SAVE_FILE))
                {
                    return new GameSaveData();
                }
                
                var es3File = new ES3File(CLOUD_SAVE_FILE, false);
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
                
                // Save to cloud_save.es3 (backup of original response)
                var es3File = new ES3File(CLOUD_SAVE_FILE, false);
                es3File.Save("levels", data.levels);
                es3File.Save("achievements", data.achievements);
                es3File.Save("lastModifiedTimestamp", data.lastModifiedTimestamp);
                es3File.Sync();
                
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
        /// Save raw JSON from API to both cloud_save.es3 (original) and local_save.es3 (copy)
        /// API returns ES3 file format, we just copy it directly - no parsing needed!
        /// </summary>
        public static bool SaveCloudJsonToLocal(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("Cannot save empty JSON");
                return false;
            }

            try
            {
                Debug.Log($"Saving raw JSON from API. Length: {json.Length}");
                
                // Save to cloud_save.es3 (original response backup)
                var cloudFile = new ES3File(CLOUD_SAVE_FILE, false);
                cloudFile.SaveRaw(System.Text.Encoding.UTF8.GetBytes(json));
                cloudFile.Sync();
                
                // Copy to local_save.es3 (1:1 copy for gameplay)
                var localFile = new ES3File(LOCAL_SAVE_FILE, false);
                localFile.SaveRaw(System.Text.Encoding.UTF8.GetBytes(json));
                localFile.Sync();
                
                // Verify it was saved
                var testData = LoadLocal();
                Debug.Log($"Successfully saved cloud JSON: {testData.levels.Count} levels, {testData.achievements.Count} achievements");
                
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
        /// Get local save as raw JSON string for API upload (ES3 format)
        /// Reads from local_save.es3
        /// </summary>
        public static string GetLocalSaveAsJson()
        {
            try
            {
                if (!ES3.FileExists(LOCAL_SAVE_FILE))
                {
                    return "{}";
                }
                
                // Use ES3File to get the raw JSON string from local_save.es3
                var es3File = new ES3File(LOCAL_SAVE_FILE, false);
                string rawJson = es3File.LoadRawString();
                
                // If empty, return empty JSON object
                if (string.IsNullOrEmpty(rawJson))
                {
                    return "{}";
                }
                
                return rawJson;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get local save as JSON: {e.Message}");
                // Fallback: serialize the data object
                try
                {
                    var localData = LoadLocal();
                    return ParadoxNotion.Serialization.JSONSerializer.Serialize(typeof(GameSaveData), localData, null, false);
                }
                catch
                {
                    return "{}";
                }
            }
        }

        /// <summary>
        /// Clear all saves (for testing/reset)
        /// </summary>
        public static void ClearAllSaves()
        {
            // Delete save files
            if (ES3.FileExists(LOCAL_SAVE_FILE))
                ES3.DeleteFile(LOCAL_SAVE_FILE);
            if (ES3.FileExists(CLOUD_SAVE_FILE))
                ES3.DeleteFile(CLOUD_SAVE_FILE);
            
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

