using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using GameEvents;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Save")]
    [Description("Upload save data to cloud API. Simple upload without sync logic. Only works if logged in.")]
    public class UploadSaveDataAction : ActionTask
    {
        [Tooltip("Optional: Event to trigger on upload complete")]
        public BBParameter<string> onUploadCompleteEventName;

        [Tooltip("Output: Success status")]
        public BBParameter<bool> outSuccess;

        [Tooltip("Output: Error message if failed")]
        public BBParameter<string> outError;

        private Coroutine uploadCoroutine;

        protected override string info => "Upload Save Data";

        protected override void OnExecute()
        {
            // Check if logged in
            if (!LoginManager.IsLoggedIn())
            {
                Debug.LogWarning("Cannot upload: User is not logged in");
                if (outSuccess != null) outSuccess.value = false;
                if (outError != null) outError.value = "Not logged in";
                EndAction(false);
                return;
            }

            uploadCoroutine = CoroutineHelper.StartStaticCoroutine(UploadCoroutine());
        }

        private IEnumerator UploadCoroutine()
        {
            int primaryId = LoginManager.GetStudentPrimaryID();
            string studentId = LoginManager.GetStudentID();
            string password = LoginManager.GetPassword();

            if (primaryId <= 0)
            {
                Debug.LogError("No primary ID found for upload. User may need to login again.");
                if (outSuccess != null) outSuccess.value = false;
                if (outError != null) outError.value = "No primary ID - please login again";
                EndAction(false);
                yield break;
            }

            if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(password))
            {
                Debug.LogError("No credentials found for upload");
                if (outSuccess != null) outSuccess.value = false;
                if (outError != null) outError.value = "No credentials";
                EndAction(false);
                yield break;
            }

            // Upload to API using existing coroutine
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


            if (outSuccess != null) outSuccess.value = true;
            if (outError != null) outError.value = "";

            if (onUploadCompleteEventName != null && !string.IsNullOrEmpty(onUploadCompleteEventName.value))
            {
                UIEventManager.Trigger(onUploadCompleteEventName.value);
            }

            EndAction(true);
        }

        protected override void OnStop()
        {
            if (uploadCoroutine != null)
            {
                CoroutineHelper.StopStaticCoroutine(uploadCoroutine);
            }
        }
    }
}

