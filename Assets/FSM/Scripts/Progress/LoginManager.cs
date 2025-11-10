using System;
using UnityEngine;

namespace GameProgress
{
    /// <summary>
    /// Manages user login state and credentials storage using ES3
    /// </summary>
    public static class LoginManager
    {
        private const string LOGIN_STATE_KEY = "GameSave_IsLoggedIn";
        private const string STUDENT_ID_KEY = "GameSave_StudentID";
        private const string STUDENT_PRIMARY_ID_KEY = "GameSave_StudentPrimaryID"; // The numeric ID from login response
        private const string PASSWORD_KEY = "GameSave_Password";
        private const string ES3_FILE = "LoginData.es3";

        // Events
        public static event Action OnLoginSuccess;
        public static event Action OnLogout;

        /// <summary>
        /// Check if user is logged in
        /// </summary>
        public static bool IsLoggedIn()
        {
            if (!ES3.KeyExists(LOGIN_STATE_KEY, ES3_FILE))
                return false;
            
            return ES3.Load<bool>(LOGIN_STATE_KEY, ES3_FILE, false);
        }

        /// <summary>
        /// Check if user has cloud save enabled (logged in)
        /// </summary>
        public static bool HasCloudSave()
        {
            return IsLoggedIn();
        }

        /// <summary>
        /// Get stored student ID (the string student_id like "17-2168-338")
        /// </summary>
        public static string GetStudentID()
        {
            if (!ES3.KeyExists(STUDENT_ID_KEY, ES3_FILE))
                return "";
            
            return ES3.Load<string>(STUDENT_ID_KEY, ES3_FILE, "");
        }

        /// <summary>
        /// Get stored student primary ID (the numeric ID from login response, e.g., 3)
        /// This is the ID used in progress API endpoints
        /// </summary>
        public static int GetStudentPrimaryID()
        {
            if (!ES3.KeyExists(STUDENT_PRIMARY_ID_KEY, ES3_FILE))
                return 0;
            
            return ES3.Load<int>(STUDENT_PRIMARY_ID_KEY, ES3_FILE, 0);
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
        /// </summary>
        public static void SetLoggedIn(string studentId, string password, int primaryId = 0)
        {
            ES3.Save(LOGIN_STATE_KEY, true, ES3_FILE);
            ES3.Save(STUDENT_ID_KEY, studentId, ES3_FILE);
            ES3.Save(PASSWORD_KEY, password, ES3_FILE);
            if (primaryId > 0)
            {
                ES3.Save(STUDENT_PRIMARY_ID_KEY, primaryId, ES3_FILE);
            }

            OnLoginSuccess?.Invoke();
        }

        /// <summary>
        /// Logout and clear credentials
        /// </summary>
        public static void Logout()
        {
            ES3.Save(LOGIN_STATE_KEY, false, ES3_FILE);
            
            if (ES3.KeyExists(STUDENT_ID_KEY, ES3_FILE))
                ES3.DeleteKey(STUDENT_ID_KEY, ES3_FILE);
            if (ES3.KeyExists(STUDENT_PRIMARY_ID_KEY, ES3_FILE))
                ES3.DeleteKey(STUDENT_PRIMARY_ID_KEY, ES3_FILE);
            if (ES3.KeyExists(PASSWORD_KEY, ES3_FILE))
                ES3.DeleteKey(PASSWORD_KEY, ES3_FILE);
            
            OnLogout?.Invoke();
            Debug.Log("Logged out");
        }

        /// <summary>
        /// Clear all login data (for testing/reset)
        /// </summary>
        public static void ClearAllLoginData()
        {
            if (ES3.KeyExists(LOGIN_STATE_KEY, ES3_FILE))
                ES3.DeleteKey(LOGIN_STATE_KEY, ES3_FILE);
            if (ES3.KeyExists(STUDENT_ID_KEY, ES3_FILE))
                ES3.DeleteKey(STUDENT_ID_KEY, ES3_FILE);
            if (ES3.KeyExists(STUDENT_PRIMARY_ID_KEY, ES3_FILE))
                ES3.DeleteKey(STUDENT_PRIMARY_ID_KEY, ES3_FILE);
            if (ES3.KeyExists(PASSWORD_KEY, ES3_FILE))
                ES3.DeleteKey(PASSWORD_KEY, ES3_FILE);
        }
    }
}

