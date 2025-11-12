using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using GameEvents;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Login")]
    [Description("Sync on menu: Download from cloud, compare with local. If different, upload local to cloud (overwrite). If same, skip.")]
    public class SyncOnMenuAction : ActionTask
    {
        [Tooltip("Optional: Event to trigger on sync complete")]
        public BBParameter<string> onSyncCompleteEventName;

        [Tooltip("Output: Success status")]
        public BBParameter<bool> outSuccess;

        [Tooltip("Output: Error message if failed")]
        public BBParameter<string> outError;

        [Tooltip("Output: Whether data was different and upload was performed")]
        public BBParameter<bool> outDataWasDifferent;

        private Coroutine syncCoroutine;

        protected override string info => "Sync On Menu (Compare & Upload if Different)";

        protected override void OnExecute()
        {
            // Check if logged in
            if (!LoginManager.IsLoggedIn())
            {
                Debug.LogWarning("Cannot sync: User is not logged in");
                if (outSuccess != null) outSuccess.value = false;
                if (outError != null) outError.value = "Not logged in";
                if (outDataWasDifferent != null) outDataWasDifferent.value = false;
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
                if (outDataWasDifferent != null) outDataWasDifferent.value = false;
                EndAction(false);
                yield break;
            }

            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(password))
            {
                Debug.LogError("No credentials found for sync");
                if (outSuccess != null) outSuccess.value = false;
                if (outError != null) outError.value = "No credentials";
                if (outDataWasDifferent != null) outDataWasDifferent.value = false;
                EndAction(false);
                yield break;
            }

            // Step 1: Download latest from API
            Debug.Log("Step 1: Downloading latest data from cloud...");
            bool downloadSuccess = false;
            string apiResponseJson = "";
            yield return GameSaveAPIManager.DownloadSaveDataCoroutine(primaryId, studentId, password, (s, data, m) =>
            {
                downloadSuccess = s;
                apiResponseJson = m; // m is the raw JSON response string
            });

            if (!downloadSuccess)
            {
                Debug.LogError($"Download failed: {apiResponseJson}");
                if (outSuccess != null) outSuccess.value = false;
                if (outError != null) outError.value = $"Download failed: {apiResponseJson}";
                if (outDataWasDifferent != null) outDataWasDifferent.value = false;
                EndAction(false);
                yield break;
            }

            Debug.Log("Step 1: Downloaded from cloud");

            // Step 2: Compare API response JSON with cloud_save.json
            bool dataIsDifferent = GameSaveManager.IsCloudResponseDifferent(apiResponseJson);
            
            if (outDataWasDifferent != null)
                outDataWasDifferent.value = dataIsDifferent;

            if (dataIsDifferent)
            {
                Debug.Log("Step 2: API response and cloud_save.json are different - uploading cloud_save.json to cloud...");
                
                // Step 3: Upload cloud_save.json to API (overwrite)
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

                Debug.Log("Step 3: Uploaded cloud_save.json to cloud (overwritten)");
            }
            else
            {
                Debug.Log("Step 2: API response and cloud_save.json are the same - skipping upload");
            }

            // Step 4: API response is already saved to cloud_save.json by DownloadSaveDataCoroutine
            Debug.Log("Step 4: Cloud save is up to date");

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

