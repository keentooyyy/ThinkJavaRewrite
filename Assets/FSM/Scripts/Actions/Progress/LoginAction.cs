using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using GameEvents;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Login")]
    [Description("Login to API with student_id and password. Saves credentials if successful.")]
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
            int primaryId = 0;

            yield return GameSaveAPIManager.LoginCoroutine(studentId.value, password.value, (s, msg, id) =>
            {
                success = s;
                error = msg;
                primaryId = id;
            });

            if (success)
            {
                // Save credentials and set logged in with primary ID
                LoginManager.SetLoggedIn(studentId.value, password.value, primaryId);
                
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

        protected override void OnStop()
        {
            if (loginCoroutine != null)
            {
                CoroutineHelper.StopStaticCoroutine(loginCoroutine);
            }
        }
    }
}

