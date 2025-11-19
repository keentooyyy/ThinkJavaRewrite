using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

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
        /// Manually parses ES3 format JSON to handle type mismatches
        /// </summary>
        public static GameSaveData LoadCloud()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, CLOUD_SAVE_FILE);
                
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[LoadCloud] File does not exist: {filePath}");
                    return new GameSaveData();
                }
                
                string json = File.ReadAllText(filePath);
                
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning("[LoadCloud] JSON file is empty or null");
                    return new GameSaveData();
                }
                
                // Log first 200 chars of JSON for debugging
                string jsonPreview = json.Length > 200 ? json.Substring(0, 200) + "..." : json;
                
                var data = new GameSaveData();
                
                // Use ES3File to parse the JSON, then manually extract nested "value" structures
                const string TEMP_FILE = "temp_cloud_load.es3";
                string tempFilePath = Path.Combine(Application.persistentDataPath, TEMP_FILE);
                
                try
                {
                    // Write JSON to temp ES3 file
                    File.WriteAllText(tempFilePath, json);
                    
                    // Use ES3File to load the raw structure
                    var es3File = new ES3File(TEMP_FILE, false);
                    
                    // Always try manual extraction first since ES3File might not parse the JSON structure correctly
                    // The JSON has ES3 format with "__type" and "value" nested structure
                    try
                    {
                        string levelsJson = ExtractValueFromES3Json(json, "levels");
                        
                        if (!string.IsNullOrEmpty(levelsJson))
                        {
                            string levelsPreview = levelsJson.Length > 300 ? levelsJson.Substring(0, 300) + "..." : levelsJson;
                            
                            // Parse using Newtonsoft since JsonUtility can't handle dictionaries
                            data.levels = DeserializeDictionary<LevelData>(levelsJson, "levels");
                            
                            // Log each level
                            foreach (var kvp in data.levels)
                            {
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[LoadCloud] Failed to extract levels JSON, trying ES3File direct load...");
                            // Fallback: try loading directly from ES3File
                            if (es3File.KeyExists("levels"))
                            {
                                data.levels = es3File.Load<Dictionary<string, LevelData>>("levels", new Dictionary<string, LevelData>());
                            }
                            else
                            {
                                Debug.LogWarning("[LoadCloud] 'levels' key not found in ES3File either");
                                data.levels = new Dictionary<string, LevelData>();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[LoadCloud] Failed to load levels using manual extraction: {e.Message}");
                        Debug.LogWarning($"[LoadCloud] Exception stack trace: {e.StackTrace}");
                        data.levels = new Dictionary<string, LevelData>();
                    }
                    
                    // Load achievements - always try manual extraction first
                    try
                    {
                        string achievementsJson = ExtractValueFromES3Json(json, "achievements");
                        
                        if (!string.IsNullOrEmpty(achievementsJson))
                        {
                            string achievementsPreview = achievementsJson.Length > 300 ? achievementsJson.Substring(0, 300) + "..." : achievementsJson;
                            
                            // Parse achievements JSON using Newtonsoft
                            data.achievements = DeserializeDictionary<AchievementSaveData>(achievementsJson, "achievements");
                            
                            // Log each achievement
                            foreach (var kvp in data.achievements)
                            {
                            }
                        }
                        else
                        {
                            Debug.LogWarning("[LoadCloud] Failed to extract achievements JSON, trying ES3File direct load...");
                            if (es3File.KeyExists("achievements"))
                            {
                                data.achievements = es3File.Load<Dictionary<string, AchievementSaveData>>("achievements", new Dictionary<string, AchievementSaveData>());
                            }
                            else
                            {
                                Debug.LogWarning("[LoadCloud] 'achievements' key not found in ES3File either");
                                data.achievements = new Dictionary<string, AchievementSaveData>();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[LoadCloud] Failed to load achievements: {e.Message}");
                        Debug.LogWarning($"[LoadCloud] Exception stack trace: {e.StackTrace}");
                        data.achievements = new Dictionary<string, AchievementSaveData>();
                    }
                    
                    // Load timestamp - try ES3File first, then manual extraction
                    if (es3File.KeyExists("lastModifiedTimestamp"))
                    {
                        data.lastModifiedTimestamp = es3File.Load<long>("lastModifiedTimestamp", 0);
                    }
                    else
                    {
                        Debug.LogWarning("[LoadCloud] 'lastModifiedTimestamp' key not found in ES3File, using default 0");
                        data.lastModifiedTimestamp = 0;
                    }
                }
                finally
                {
                    // Clean up temp file
                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }
                    if (ES3.FileExists(TEMP_FILE))
                    {
                        ES3.DeleteFile(TEMP_FILE);
                    }
                }
                
                // Ensure dictionaries are initialized
                if (data.levels == null)
                {
                    Debug.LogWarning("[LoadCloud] levels dictionary was null, initializing empty dictionary");
                    data.levels = new Dictionary<string, LevelData>();
                }
                if (data.achievements == null)
                {
                    Debug.LogWarning("[LoadCloud] achievements dictionary was null, initializing empty dictionary");
                    data.achievements = new Dictionary<string, AchievementSaveData>();
                }
                
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoadCloud] Failed to load cloud save: {e.Message}");
                Debug.LogError($"[LoadCloud] Stack trace: {e.StackTrace}");
                return new GameSaveData();
            }
        }
        
        /// <summary>
        /// Extract the "value" JSON from an ES3 format structure
        /// ES3 format: "key": { "__type": "...", "value": { ... } }
        /// </summary>
        private static string ExtractValueFromES3Json(string json, string key)
        {
            try
            {
                int keyStart = json.IndexOf($"\"{key}\"", StringComparison.Ordinal);
                if (keyStart < 0)
                {
                    Debug.LogWarning($"[ExtractValueFromES3Json] Key '{key}' not found in JSON");
                    return null;
                }
                
                int valueStart = json.IndexOf("\"value\"", keyStart, StringComparison.Ordinal);
                if (valueStart < 0)
                {
                    Debug.LogWarning($"[ExtractValueFromES3Json] 'value' keyword not found after key '{key}'");
                    return null;
                }
                
                int valueObjStart = json.IndexOf("{", valueStart, StringComparison.Ordinal);
                if (valueObjStart < 0)
                {
                    Debug.LogWarning("[ExtractValueFromES3Json] Value object start '{' not found");
                    return null;
                }
                
                // Find matching closing brace
                int braceCount = 0;
                int valueObjEnd = valueObjStart;
                for (int i = valueObjStart; i < json.Length; i++)
                {
                    if (json[i] == '{') braceCount++;
                    if (json[i] == '}') braceCount--;
                    if (braceCount == 0)
                    {
                        valueObjEnd = i + 1;
                        break;
                    }
                }
                
                string extracted = json.Substring(valueObjStart, valueObjEnd - valueObjStart);
                string preview = extracted.Length > 200 ? extracted.Substring(0, 200) + "..." : extracted;
                
                return extracted;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ExtractValueFromES3Json] Failed to extract value from ES3 JSON for key '{key}': {e.Message}");
                Debug.LogError($"[ExtractValueFromES3Json] Stack trace: {e.StackTrace}");
                return null;
            }
        }

        private static Dictionary<string, T> DeserializeDictionary<T>(string json, string label) where T : class
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"[DeserializeDictionary] JSON for '{label}' is null or empty");
                return new Dictionary<string, T>();
            }
            
            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, T>>(json);
                if (dict == null)
                {
                    Debug.LogWarning($"[DeserializeDictionary] Newtonsoft returned null for '{label}'");
                    return new Dictionary<string, T>();
                }
                
                return dict;
            }
            catch (Exception e)
            {
                Debug.LogError($"[DeserializeDictionary] Failed to parse '{label}' dictionary: {e.Message}");
                Debug.LogError($"[DeserializeDictionary] Stack trace: {e.StackTrace}");
                return new Dictionary<string, T>();
            }
        }

        /// <summary>
        /// Fallback: Manually parse levels from ES3 JSON format
        /// </summary>

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


