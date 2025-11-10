using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using GameEvents;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Login")]
    [Description("Sync on login: Push local to cloud, upload to API, download from API, pull cloud to local. Only works if logged in.")]
    public class SyncOnLoginAction : ActionTask
    {
        [Tooltip("Optional: Event to trigger on sync complete")]
        public BBParameter<string> onSyncCompleteEventName;

        [Tooltip("Output: Success status")]
        public BBParameter<bool> outSuccess;

        [Tooltip("Output: Error message if failed")]
        public BBParameter<string> outError;

        private Coroutine syncCoroutine;

        protected override string info => "Sync On Login";

        protected override void OnExecute()
        {
            // Check if logged in
            if (!LoginManager.IsLoggedIn())
            {
                Debug.LogWarning("Cannot sync: User is not logged in");
                if (outSuccess != null) outSuccess.value = false;
                if (outError != null) outError.value = "Not logged in";
                EndAction(false);
                return;
            }

            syncCoroutine = CoroutineHelper.StartStaticCoroutine(SyncCoroutine());
        }

        private IEnumerator SyncCoroutine()
        {
            int primaryId = LoginManager.GetStudentPrimaryID();
            string studentId = LoginManager.GetStudentID();
            string password = LoginManager.GetPassword();

            if (primaryId <= 0)
            {
                Debug.LogError("No primary ID found for sync. User may need to login again.");
                if (outSuccess != null) outSuccess.value = false;
                if (outError != null) outError.value = "No primary ID - please login again";
                EndAction(false);
                yield break;
            }

            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(password))
            {
                Debug.LogError("No credentials found for sync");
                if (outSuccess != null) outSuccess.value = false;
                if (outError != null) outError.value = "No credentials";
                EndAction(false);
                yield break;
            }

            // Step 1: Push local to cloud (sync local changes)
            GameSaveManager.SyncLocalToCloud();
            Debug.Log("Step 1: Synced local to cloud");

            // Step 2: Upload cloud to API (using primary ID)
            bool uploadSuccess = false;
            string uploadMessage = "";
            yield return GameSaveAPIManager.UploadSaveDataCoroutine(primaryId, studentId, password, (s, m) =>
            {
                uploadSuccess = s;
                uploadMessage = m;
            });

            if (!uploadSuccess)
            {
                Debug.LogError($"Upload failed: {uploadMessage}");
                if (outSuccess != null) outSuccess.value = false;
                if (outError != null) outError.value = $"Upload failed: {uploadMessage}";
                EndAction(false);
                yield break;
            }

            Debug.Log("Step 2: Uploaded to API");

            // Step 3: Download from API (gets latest from server, using primary ID)
            bool downloadSuccess = false;
            string downloadMessage = "";
            yield return GameSaveAPIManager.DownloadSaveDataCoroutine(primaryId, studentId, password, (s, data, m) =>
            {
                downloadSuccess = s;
                downloadMessage = m;
            });

            if (!downloadSuccess)
            {
                Debug.LogError($"Download failed: {downloadMessage}");
                if (outSuccess != null) outSuccess.value = false;
                if (outError != null) outError.value = $"Download failed: {downloadMessage}";
                EndAction(false);
                yield break;
            }

            Debug.Log("Step 3: Downloaded from API");

            // Step 4: Pull cloud to local (cloud is now authoritative)
            var cloudData = GameSaveManager.LoadCloud();
            GameSaveManager.SaveLocal(cloudData);
            Debug.Log("Step 4: Synced cloud to local (cloud is authoritative)");

            if (outSuccess != null) outSuccess.value = true;
            if (outError != null) outError.value = "";

            if (onSyncCompleteEventName != null && !string.IsNullOrEmpty(onSyncCompleteEventName.value))
            {
                UIEventManager.Trigger(onSyncCompleteEventName.value);
            }

            EndAction(true);
        }

        protected override void OnStop()
        {
            if (syncCoroutine != null)
            {
                CoroutineHelper.StopStaticCoroutine(syncCoroutine);
            }
        }
    }
}

