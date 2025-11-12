using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using GameEvents;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Login")]
    [Description("Initialize save: If logged in, sync with cloud. Otherwise, create new local save.")]
    public class HandleFirstLoginAction : ActionTask
    {
        [Tooltip("Optional: Event to trigger after initialization")]
        public BBParameter<string> onCompleteEventName;

        [Tooltip("Output: Success status")]
        public BBParameter<bool> outSuccess;

        private Coroutine initCoroutine;

        protected override string info => "Initialize Save";

        protected override void OnExecute()
        {
            // Check if logged in
            if (LoginManager.IsLoggedIn())
            {
                // Logged in - sync with cloud (download, compare, upload if different)
                initCoroutine = CoroutineHelper.StartStaticCoroutine(InitWithCloudSync());
            }
            else
            {
                // Not logged in - create new cloud save
                var newCloudData = new GameSaveData();
                GameSaveManager.SaveCloud(newCloudData);
                
                if (outSuccess != null) outSuccess.value = true;
                if (onCompleteEventName != null && !string.IsNullOrEmpty(onCompleteEventName.value))
                {
                    UIEventManager.Trigger(onCompleteEventName.value);
                }
                EndAction(true);
            }
        }

        private IEnumerator InitWithCloudSync()
        {
            int primaryId = LoginManager.GetStudentPrimaryID();
            string studentId = LoginManager.GetStudentID();
            string password = LoginManager.GetPassword();

            if (primaryId <= 0)
            {
                Debug.LogError("No primary ID found. User may need to login again.");
                if (outSuccess != null) outSuccess.value = false;
                EndAction(false);
                yield break;
            }

            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(password))
            {
                Debug.LogError("No credentials found");
                if (outSuccess != null) outSuccess.value = false;
                EndAction(false);
                yield break;
            }

            // Download from API (using primary ID)
            bool downloadSuccess = false;
            string downloadMessage = "";

            yield return GameSaveAPIManager.DownloadSaveDataCoroutine(primaryId, studentId, password, (s, data, m) =>
            {
                downloadSuccess = s;
                downloadMessage = m;
            });

            if (!downloadSuccess)
            {
                // Download failed (network error, etc.), create fresh cloud save
                Debug.LogWarning("Download failed, creating fresh cloud save");
                var newCloudData = new GameSaveData();
                GameSaveManager.SaveCloud(newCloudData);
            }
            // If download succeeded, data is already saved to cloud_save.json

            // Fire HideLoginUI event and wait for animation to complete
            UIEventManager.Trigger("HideLoginUI");
            
            // Wait for hide animation to complete
            yield return new UnityEngine.WaitForSeconds(0.5f);

            if (outSuccess != null) outSuccess.value = true;

            if (onCompleteEventName != null && !string.IsNullOrEmpty(onCompleteEventName.value))
            {
                UIEventManager.Trigger(onCompleteEventName.value);
            }

            EndAction(true);
        }

        protected override void OnStop()
        {
            if (initCoroutine != null)
            {
                CoroutineHelper.StopStaticCoroutine(initCoroutine);
            }
        }
    }
}

