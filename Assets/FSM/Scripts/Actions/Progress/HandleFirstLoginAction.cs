using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using GameEvents;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Login")]
    [Description("Handle first login: If logged in, download from API. Otherwise, create new local save.")]
    public class HandleFirstLoginAction : ActionTask
    {
        [Tooltip("Optional: Event to trigger after first login handling")]
        public BBParameter<string> onCompleteEventName;

        [Tooltip("Output: Success status")]
        public BBParameter<bool> outSuccess;

        private Coroutine firstLoginCoroutine;

        protected override string info => "Handle First Login";

        protected override void OnExecute()
        {
            if (!GameSaveManager.IsFirstLogin())
            {
                Debug.Log("Not first login, skipping");
                if (outSuccess != null) outSuccess.value = true;
                EndAction(true);
                return;
            }

            // Check if logged in
            if (LoginManager.IsLoggedIn())
            {
                // Logged in - download from API
                firstLoginCoroutine = CoroutineHelper.StartStaticCoroutine(FirstLoginWithAPI());
            }
            else
            {
                // Not logged in - create new local save
                var newLocalData = new GameSaveData();
                GameSaveManager.SaveLocal(newLocalData);
                GameSaveManager.HandleFirstLogin(); // Mark first login complete
                
                if (outSuccess != null) outSuccess.value = true;
                if (onCompleteEventName != null && !string.IsNullOrEmpty(onCompleteEventName.value))
                {
                    UIEventManager.Trigger(onCompleteEventName.value);
                }
                EndAction(true);
            }
        }

        private IEnumerator FirstLoginWithAPI()
        {
            int primaryId = LoginManager.GetStudentPrimaryID();
            string studentId = LoginManager.GetStudentID();
            string password = LoginManager.GetPassword();

            if (primaryId <= 0)
            {
                Debug.LogError("No primary ID found for first login. User may need to login again.");
                if (outSuccess != null) outSuccess.value = false;
                EndAction(false);
                yield break;
            }

            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(password))
            {
                Debug.LogError("No credentials found for first login");
                if (outSuccess != null) outSuccess.value = false;
                EndAction(false);
                yield break;
            }

            // Download from API (using primary ID)
            bool downloadSuccess = false;
            string downloadMessage = "";
            GameSaveData downloadedData = null;

            yield return GameSaveAPIManager.DownloadSaveDataCoroutine(primaryId, studentId, password, (s, data, m) =>
            {
                downloadSuccess = s;
                downloadedData = data;
                downloadMessage = m;
            });

            if (downloadSuccess && downloadedData != null)
            {
                // Check if we got actual data (not empty)
                bool hasData = (downloadedData.levels != null && downloadedData.levels.Count > 0) ||
                              (downloadedData.achievements != null && downloadedData.achievements.Count > 0);

                if (hasData)
                {
                    // Cloud save exists, overwrite local
                    Debug.Log("Cloud save found - overwriting local save");
                    GameSaveManager.SaveLocal(downloadedData);
                }
                else
                {
                    // No cloud save, create fresh local save
                    Debug.Log("No cloud save found - creating new local save");
                    var newLocalData = new GameSaveData();
                    GameSaveManager.SaveLocal(newLocalData);
                }
            }
            else
            {
                // Download failed (network error, etc.), create fresh local save
                Debug.LogWarning($"Could not download from API: {downloadMessage}. Creating new local save.");
                var newLocalData = new GameSaveData();
                GameSaveManager.SaveLocal(newLocalData);
            }

            // Mark first login as complete
            GameSaveManager.HandleFirstLogin();

            if (outSuccess != null) outSuccess.value = true;

            if (onCompleteEventName != null && !string.IsNullOrEmpty(onCompleteEventName.value))
            {
                UIEventManager.Trigger(onCompleteEventName.value);
            }

            EndAction(true);
        }

        protected override void OnStop()
        {
            if (firstLoginCoroutine != null)
            {
                CoroutineHelper.StopStaticCoroutine(firstLoginCoroutine);
            }
        }
    }
}

