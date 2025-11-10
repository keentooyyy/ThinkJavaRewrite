using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using ParadoxNotion.Serialization;

namespace GameProgress
{
    /// <summary>
    /// Manages API communication with Django backend using form data authentication
    /// Endpoints:
    /// - POST /api/student_login/ - Login (student_id, password) - Returns student primary ID
    /// - GET /api/progress/<id>/ - Get save data (student_id, password in form) - Uses primary ID
    /// - POST /api/progress/update/<id>/ - Update save data (student_id, password, payload in form) - Uses primary ID
    /// </summary>
    public static class GameSaveAPIManager
    {
        /// <summary>
        /// Login response structure from Django API
        /// </summary>
        [Serializable]
        private class LoginResponse
        {
            public string status;
            public StudentData student;
        }

        [Serializable]
        private class StudentData
        {
            public int id; // Primary ID used in progress endpoints
            public string student_id; // String student ID like "17-2168-338"
            public string first_name;
            public string last_name;
            public string role;
        }
        // API Configuration
        private static string apiBaseUrl = "https://your-api-domain.com/api";
        private static int timeoutSeconds = 30;

        // Events
        public static event Action<string> OnUploadSuccess;
        public static event Action<string> OnUploadFailed;
        public static event Action<GameSaveData> OnDownloadSuccess;
        public static event Action<string> OnDownloadFailed;
        public static event Action OnLoginSuccess;
        public static event Action<string> OnLoginFailed;

        /// <summary>
        /// Set API base URL
        /// </summary>
        public static void SetAPIBaseUrl(string url)
        {
            apiBaseUrl = url.TrimEnd('/');
            Debug.Log($"API Base URL set to: {apiBaseUrl}");
        }

        /// <summary>
        /// Set request timeout in seconds
        /// </summary>
        public static void SetTimeout(int seconds)
        {
            timeoutSeconds = seconds;
        }

        /// <summary>
        /// Login to API (POST /api/student_login/)
        /// Returns the primary student ID in the onComplete callback as the second parameter (if successful)
        /// </summary>
        public static IEnumerator LoginCoroutine(string studentId, string password, Action<bool, string, int> onComplete = null)
        {
            string url = $"{apiBaseUrl}/student_login/";
            
            WWWForm form = new WWWForm();
            form.AddField("student_id", studentId);
            form.AddField("password", password);
            
            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.timeout = timeoutSeconds;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"Login successful. Response: {responseText}");
                
                // Parse the login response to extract the primary ID
                int primaryId = 0;
                try
                {
                    var loginResponse = JsonUtility.FromJson<LoginResponse>(responseText);
                    if (loginResponse != null && loginResponse.student != null && loginResponse.student.id > 0)
                    {
                        primaryId = loginResponse.student.id;
                        Debug.Log($"Extracted primary ID from login response: {primaryId}");
                    }
                    else
                    {
                        Debug.LogWarning("Failed to parse primary ID from login response");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing login response: {e.Message}");
                }
                
                OnLoginSuccess?.Invoke();
                onComplete?.Invoke(true, responseText, primaryId);
            }
            else
            {
                string error = $"Login failed: {request.error} (Code: {request.responseCode})";
                Debug.LogError(error);
                OnLoginFailed?.Invoke(error);
                onComplete?.Invoke(false, error, 0);
            }

            request.Dispose();
        }

        /// <summary>
        /// Download save data from API (GET /api/progress/<id>/)
        /// Uses the primary student ID (not student_id string) in the URL
        /// Uses POST to send form data (student_id and password) as per Django API requirements
        /// If POST fails with 404, tries GET as fallback (some Django setups use GET)
        /// </summary>
        public static IEnumerator DownloadSaveDataCoroutine(int primaryId, string studentId, string password, Action<bool, GameSaveData, string> onComplete = null)
        {
            if (primaryId <= 0)
            {
                Debug.LogError("Invalid primary ID for download. Must be > 0.");
                onComplete?.Invoke(false, null, "Invalid primary ID");
                yield break;
            }
            
            string url = $"{apiBaseUrl}/progress/{primaryId}/";
            
            Debug.Log($"Downloading save data from: {url}");
            Debug.Log($"Primary ID: {primaryId}, Student ID: {studentId}, Password: {(string.IsNullOrEmpty(password) ? "EMPTY" : "***")}");
            
            // Try POST first (as per API docs with form data)
            WWWForm form = new WWWForm();
            form.AddField("student_id", studentId);
            form.AddField("password", password);
            
            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.timeout = timeoutSeconds;

            yield return request.SendWebRequest();

            // If POST fails with 404 or method not allowed, try GET with query parameters
            if (request.result != UnityWebRequest.Result.Success && 
                (request.responseCode == 404 || request.responseCode == 405))
            {
                Debug.LogWarning($"POST failed with code {request.responseCode}, trying GET as fallback...");
                request.Dispose();
                
                // Try GET with query parameters
                string getUrl = $"{url}?student_id={UnityWebRequest.EscapeURL(studentId)}&password={UnityWebRequest.EscapeURL(password)}";
                UnityWebRequest getRequest = UnityWebRequest.Get(getUrl);
                getRequest.timeout = timeoutSeconds;
                
                yield return getRequest.SendWebRequest();
                request = getRequest;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"Save data downloaded successfully. Response length: {jsonResponse.Length} chars");
                
                // Log first 1000 chars for debugging
                string preview = jsonResponse.Length > 1000 ? jsonResponse.Substring(0, 1000) + "..." : jsonResponse;
                Debug.Log($"Response preview: {preview}");

                // Check if response is actually empty or error message
                if (string.IsNullOrEmpty(jsonResponse) || jsonResponse.Trim() == "{}" || jsonResponse.Trim() == "null")
                {
                    Debug.LogWarning("Received empty or null response from server");
                    var emptySaveData = new GameSaveData();
                    OnDownloadSuccess?.Invoke(emptySaveData);
                    onComplete?.Invoke(true, emptySaveData, "");
                    request.Dispose();
                    yield break;
                }

                // Save raw JSON directly to local (no parsing needed!)
                bool saveSuccess = GameSaveManager.SaveCloudJsonToLocal(jsonResponse);
                
                if (saveSuccess)
                {
                    var saveData = GameSaveManager.LoadLocal();
                    Debug.Log($"Successfully saved cloud JSON to local: {saveData.levels.Count} levels, {saveData.achievements.Count} achievements");
                    OnDownloadSuccess?.Invoke(saveData);
                    onComplete?.Invoke(true, saveData, jsonResponse);
                }
                else
                {
                    string error = $"Failed to parse downloaded save data. Response length: {jsonResponse.Length}";
                    Debug.LogError(error);
                    Debug.LogError($"Full response: {jsonResponse}");
                    OnDownloadFailed?.Invoke(error);
                    onComplete?.Invoke(false, null, error);
                }
            }
            else
            {
                string responseText = request.downloadHandler.text;
                Debug.LogWarning($"Download request failed. Code: {request.responseCode}, Error: {request.error}");
                Debug.LogWarning($"Response body: {responseText}");
                Debug.LogWarning($"Full URL attempted: {url}");
                Debug.LogWarning($"Request method: {request.method}");
                
                // Handle 404 (no save data yet) as a valid case
                if (request.responseCode == 404)
                {
                    Debug.Log("No save data found on server (404) - this is OK for new users");
                    Debug.LogWarning("NOTE: If you know this account has save data, check:");
                    Debug.LogWarning("1. The endpoint URL is correct");
                    Debug.LogWarning("2. The student_id matches exactly");
                    Debug.LogWarning("3. The password is correct");
                    Debug.LogWarning("4. The API endpoint accepts POST with form data");
                    
                    var emptySaveData = new GameSaveData();
                    OnDownloadSuccess?.Invoke(emptySaveData);
                    onComplete?.Invoke(true, emptySaveData, "");
                }
                else
                {
                    string error = $"Download failed: {request.error} (Code: {request.responseCode}). Response: {responseText}";
                    Debug.LogError(error);
                    OnDownloadFailed?.Invoke(error);
                    onComplete?.Invoke(false, null, error);
                }
            }

            request.Dispose();
        }

        /// <summary>
        /// Upload save data to API (POST /api/progress/update/<id>/)
        /// Uses the primary student ID (not student_id string) in the URL
        /// </summary>
        public static IEnumerator UploadSaveDataCoroutine(int primaryId, string studentId, string password, Action<bool, string> onComplete = null)
        {
            if (primaryId <= 0)
            {
                Debug.LogError("Invalid primary ID for upload. Must be > 0.");
                onComplete?.Invoke(false, "Invalid primary ID");
                yield break;
            }
            
            string jsonData = GameSaveManager.GetLocalSaveAsJson();
            
            if (string.IsNullOrEmpty(jsonData) || jsonData == "{}")
            {
                Debug.LogWarning("No save data to upload");
                onComplete?.Invoke(false, "No save data");
                yield break;
            }

            string url = $"{apiBaseUrl}/progress/update/{primaryId}/";
            Debug.Log($"Uploading save data to: {url} (Primary ID: {primaryId})");
            
            WWWForm form = new WWWForm();
            form.AddField("student_id", studentId);
            form.AddField("password", password);
            form.AddField("payload", jsonData);
            
            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.timeout = timeoutSeconds;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Save data uploaded successfully. Response: {request.downloadHandler.text}");
                OnUploadSuccess?.Invoke(request.downloadHandler.text);
                onComplete?.Invoke(true, request.downloadHandler.text);
            }
            else
            {
                string error = $"Upload failed: {request.error} (Code: {request.responseCode})";
                Debug.LogError(error);
                OnUploadFailed?.Invoke(error);
                onComplete?.Invoke(false, error);
            }

            request.Dispose();
        }
    }
}

