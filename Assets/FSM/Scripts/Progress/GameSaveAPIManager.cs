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
        /// Login response structure from Django API - contains all fields from the API response
        /// </summary>
        [Serializable]
        public class LoginResponse
        {
            public string status;
            public StudentData student;
            public SectionData section;
            public ProfileData profile;
            public TestStatusData test_status;
        }

        /// <summary>
        /// Student data structure from login response
        /// </summary>
        [Serializable]
        public class StudentData
        {
            public int id; // Primary ID used in progress endpoints
            public string student_id; // String student ID like "17-2168-338"
            public string first_name;
            public string last_name;
            public string role;
        }

        /// <summary>
        /// Section data from login response
        /// </summary>
        [Serializable]
        public class SectionData
        {
            public string dept;
            public int year_level;
            public string section_letter;
            public string full_section;
        }

        /// <summary>
        /// Profile data from login response
        /// </summary>
        [Serializable]
        public class ProfileData
        {
            public string middle_initial;
            public string suffix;
            public string date_of_birth;
            public int? age;
            public string bio;
            public string phone;
            public string father_name;
            public string mother_name;
            public AddressData address;
            public string profile_picture;
            public object[] education; // Can be array of education objects
        }

        /// <summary>
        /// Address data from profile
        /// </summary>
        [Serializable]
        public class AddressData
        {
            public string street;
            public string barangay;
            public string city;
            public string province;
        }

        /// <summary>
        /// Test status data from login response
        /// </summary>
        [Serializable]
        public class TestStatusData
        {
            public bool has_taken_pretest;
            public bool has_taken_posttest;
            public bool all_levels_completed;
            public bool can_take_posttest;
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
        /// Returns the primary student ID, full student data, complete login response, and HTTP response code in the onComplete callback
        /// </summary>
        public static IEnumerator LoginCoroutine(string studentId, string password, Action<bool, string, int, StudentData, LoginResponse, long> onComplete = null)
        {
            string url = $"{apiBaseUrl}/student_login/";
            
            WWWForm form = new WWWForm();
            form.AddField("student_id", studentId);
            form.AddField("password", password);
            
            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.timeout = timeoutSeconds;

            yield return request.SendWebRequest();

            long responseCode = request.responseCode;

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                
                // Parse the login response to extract the primary ID, full student data, and complete response
                int primaryId = 0;
                StudentData studentData = null;
                LoginResponse loginResponse = null;
                try
                {
                    loginResponse = JsonUtility.FromJson<LoginResponse>(responseText);
                    if (loginResponse != null && loginResponse.student != null && loginResponse.student.id > 0)
                    {
                        primaryId = loginResponse.student.id;
                        studentData = loginResponse.student;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing login response: {e.Message}");
                }
                
                OnLoginSuccess?.Invoke();
                onComplete?.Invoke(true, responseText, primaryId, studentData, loginResponse, responseCode);
            }
            else
            {
                string error = $"Login failed: {request.error} (Code: {request.responseCode})";
                Debug.LogError(error);
                OnLoginFailed?.Invoke(error);
                onComplete?.Invoke(false, error, 0, null, null, responseCode);
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

                // Save raw JSON directly to cloud_save.json (no parsing needed!)
                bool saveSuccess = GameSaveManager.SaveCloudJson(jsonResponse);
                
                if (saveSuccess)
                {
                    OnDownloadSuccess?.Invoke(null);
                    onComplete?.Invoke(true, null, jsonResponse);
                }
                else
                {
                    string error = $"Failed to save downloaded save data. Response length: {jsonResponse.Length}";
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
            
            string jsonData = GameSaveManager.GetCloudSaveAsJson();
            
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

