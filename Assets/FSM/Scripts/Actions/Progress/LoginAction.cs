using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using GameEvents;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Login")]
    [Description("Login to API with student_id and password. Saves credentials if successful. Clears blackboard variables after successful login.")]
    public class LoginAction : ActionTask
    {
        [Tooltip("Student ID (from blackboard)")]
        public BBParameter<string> studentId;

        [Tooltip("Password (from blackboard)")]
        public BBParameter<string> password;

        [Tooltip("Optional: Event to trigger on login success")]
        public BBParameter<string> onSuccessEventName;

        [Tooltip("Optional: Event to trigger on login failure")]
        public BBParameter<string> onFailureEventName;

        [Tooltip("Output: Success status")]
        public BBParameter<bool> outSuccess;

        [Tooltip("Output: Error message if failed")]
        public BBParameter<string> outError;

        private Coroutine loginCoroutine;

        protected override string info => $"Login [{studentId}]";

        protected override void OnExecute()
        {
            if (string.IsNullOrEmpty(studentId.value) || string.IsNullOrEmpty(password.value))
            {
                Debug.LogError("Student ID and password are required");
                if (outSuccess != null) outSuccess.value = false;
                if (outError != null) outError.value = "Missing credentials";
                EndAction(false);
                return;
            }

            loginCoroutine = CoroutineHelper.StartStaticCoroutine(LoginCoroutine());
        }

        private IEnumerator LoginCoroutine()
        {
            bool success = false;
            string error = "";
            string responseJson = "";
            int primaryId = 0;
            GameSaveAPIManager.LoginResponse loginResponse = null;

            yield return GameSaveAPIManager.LoginCoroutine(studentId.value, password.value, (s, msg, id, data, response) =>
            {
                success = s;
                error = msg;
                responseJson = s ? msg : ""; // msg is the response JSON when successful
                primaryId = id;
                loginResponse = response;
            });

            if (success)
            {
                // Save credentials - only password and complete LoginResponse (all other data is in LoginResponse)
                LoginManager.SetLoggedIn(password.value, loginResponse);
                
                // Clear blackboard variables after successful login
                ClearBlackboardVariables();
                
                if (outSuccess != null) outSuccess.value = true;
                if (outError != null) outError.value = "";

                if (onSuccessEventName != null && !string.IsNullOrEmpty(onSuccessEventName.value))
                {
                    UIEventManager.Trigger(onSuccessEventName.value);
                }
            }
            else
            {
                if (outSuccess != null) outSuccess.value = false;
                if (outError != null) outError.value = error;

                if (onFailureEventName != null && !string.IsNullOrEmpty(onFailureEventName.value))
                {
                    UIEventManager.Trigger(onFailureEventName.value);
                }
            }

            EndAction(success);
        }

        private void ClearBlackboardVariables()
        {
            try
            {
                // Clear studentId and password from blackboard
                if (blackboard != null)
                {
                    if (blackboard.variables.ContainsKey("studentId"))
                    {
                        blackboard.SetVariableValue("studentId", "");
                    }
                    if (blackboard.variables.ContainsKey("password"))
                    {
                        blackboard.SetVariableValue("password", "");
                    }
                }
                
                // Also clear the BBParameter values directly
                if (studentId != null)
                    studentId.value = "";
                if (password != null)
                    password.value = "";
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"LoginAction: Failed to clear blackboard variables: {e.Message}");
            }
        }

        protected override void OnStop()
        {
            if (loginCoroutine != null)
            {
                CoroutineHelper.StopStaticCoroutine(loginCoroutine);
            }
        }
    }
}

