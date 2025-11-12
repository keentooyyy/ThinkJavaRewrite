using System;
using UnityEngine;

namespace GameProgress
{
    /// <summary>
    /// Manages user login state and credentials storage using ES3
    /// All data is stored in LoginResponse object, except password (not in API response, needed for API calls)
    /// </summary>
    public static class LoginManager
    {
        private const string PASSWORD_KEY = "GameSave_Password"; // Password not in API response, needed for API calls
        private const string LOGIN_RESPONSE_KEY = "GameSave_LoginResponse"; // Complete login response with all fields
        private const string ES3_FILE = "LoginData.es3";

        // Events
        public static event Action OnLoginSuccess;
        public static event Action OnLogout;

        /// <summary>
        /// Check if user is logged in (checks if LoginResponse exists)
        /// </summary>
        public static bool IsLoggedIn()
        {
            return ES3.KeyExists(LOGIN_RESPONSE_KEY, ES3_FILE);
        }

        /// <summary>
        /// Check if user has cloud save enabled (logged in)
        /// </summary>
        public static bool HasCloudSave()
        {
            return IsLoggedIn();
        }

        /// <summary>
        /// Get stored student ID (the string student_id like "17-2168-338") - extracted from LoginResponse
        /// </summary>
        public static string GetStudentID()
        {
            var loginResponse = GetLoginResponse();
            return loginResponse?.student?.student_id ?? "";
        }

        /// <summary>
        /// Get stored student primary ID (the numeric ID from login response, e.g., 3) - extracted from LoginResponse
        /// This is the ID used in progress API endpoints
        /// </summary>
        public static int GetStudentPrimaryID()
        {
            var loginResponse = GetLoginResponse();
            return loginResponse?.student?.id ?? 0;
        }

        /// <summary>
        /// Get stored password
        /// </summary>
        public static string GetPassword()
        {
            if (!ES3.KeyExists(PASSWORD_KEY, ES3_FILE))
                return "";
            
            return ES3.Load<string>(PASSWORD_KEY, ES3_FILE, "");
        }

        /// <summary>
        /// Set login state and save credentials
        /// Saves only the complete LoginResponse object (contains all fields: status, student, section, profile, test_status)
        /// Password is saved separately as it's not in the API response but needed for API calls
        /// </summary>
        public static void SetLoggedIn(string password, GameSaveAPIManager.LoginResponse loginResponse)
        {
            if (loginResponse == null)
            {
                Debug.LogError("LoginManager.SetLoggedIn: loginResponse cannot be null");
                return;
            }
            
            // Save password (not in API response, but needed for API calls)
            ES3.Save(PASSWORD_KEY, password, ES3_FILE);
            
            // Save the complete parsed response object (contains ALL fields: status, student, section, profile, test_status)
            ES3.Save(LOGIN_RESPONSE_KEY, loginResponse, ES3_FILE);

            OnLoginSuccess?.Invoke();
        }

        /// <summary>
        /// Get stored student data (extracted from login response for convenience)
        /// </summary>
        public static GameSaveAPIManager.StudentData GetStudentData()
        {
            var loginResponse = GetLoginResponse();
            return loginResponse?.student;
        }

        /// <summary>
        /// Get stored complete login response object (contains status, student, section, profile, test_status)
        /// All other data can be extracted from this object
        /// </summary>
        public static GameSaveAPIManager.LoginResponse GetLoginResponse()
        {
            if (!ES3.KeyExists(LOGIN_RESPONSE_KEY, ES3_FILE))
                return null;
            
            return ES3.Load<GameSaveAPIManager.LoginResponse>(LOGIN_RESPONSE_KEY, ES3_FILE, default(GameSaveAPIManager.LoginResponse));
        }

        /// <summary>
        /// Logout and clear credentials
        /// </summary>
        public static void Logout()
        {
            if (ES3.KeyExists(PASSWORD_KEY, ES3_FILE))
                ES3.DeleteKey(PASSWORD_KEY, ES3_FILE);
            if (ES3.KeyExists(LOGIN_RESPONSE_KEY, ES3_FILE))
                ES3.DeleteKey(LOGIN_RESPONSE_KEY, ES3_FILE);
            
            OnLogout?.Invoke();
            Debug.Log("Logged out");
        }

        /// <summary>
        /// Clear all login data (for testing/reset)
        /// </summary>
        public static void ClearAllLoginData()
        {
            if (ES3.KeyExists(PASSWORD_KEY, ES3_FILE))
                ES3.DeleteKey(PASSWORD_KEY, ES3_FILE);
            if (ES3.KeyExists(LOGIN_RESPONSE_KEY, ES3_FILE))
                ES3.DeleteKey(LOGIN_RESPONSE_KEY, ES3_FILE);
        }
    }
}

