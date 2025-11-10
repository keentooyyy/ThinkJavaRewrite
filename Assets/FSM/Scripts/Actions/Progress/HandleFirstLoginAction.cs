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
                GameSaveManager.MarkFirstLoginComplete();
                
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

            yield return GameSaveAPIManager.DownloadSaveDataCoroutine(primaryId, studentId, password, (s, data, m) =>
            {
                downloadSuccess = s;
                downloadMessage = m;
            });

            if (downloadSuccess)
            {
                // File was saved successfully - just mark first login complete
                GameSaveManager.MarkFirstLoginComplete();
            }
            else
            {
                // Download failed (network error, etc.), create fresh local save
                var newLocalData = new GameSaveData();
                GameSaveManager.SaveLocal(newLocalData);
                GameSaveManager.MarkFirstLoginComplete();
            }

            // Fire HideLoginUI event and wait for animation to complete
            UIEventManager.Trigger("HideLoginUI");
            
            // Wait for hide animation to complete (0.8s base * 0.5 multiplier = 0.4s + buffer)
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
            if (firstLoginCoroutine != null)
            {
                CoroutineHelper.StopStaticCoroutine(firstLoginCoroutine);
            }
        }
    }
}

