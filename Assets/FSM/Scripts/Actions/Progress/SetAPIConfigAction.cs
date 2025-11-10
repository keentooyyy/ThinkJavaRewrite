using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Login")]
    [Description("Set API base URL and timeout. Call this on initialization.")]
    public class SetAPIConfigAction : ActionTask
    {
        [RequiredField]
        [Tooltip("API base URL (e.g., https://api.yourdomain.com/api)")]
        public BBParameter<string> apiBaseUrl;

        [Tooltip("Request timeout in seconds (default: 30)")]
        public BBParameter<int> timeoutSeconds = new BBParameter<int> { value = 30 };

        protected override string info => $"Set API Config [{apiBaseUrl}]";

        protected override void OnExecute()
        {
            if (string.IsNullOrEmpty(apiBaseUrl.value))
            {
                Debug.LogError("API base URL cannot be empty");
                EndAction(false);
                return;
            }

            GameSaveAPIManager.SetAPIBaseUrl(apiBaseUrl.value);
            GameSaveAPIManager.SetTimeout(timeoutSeconds.value);
            
            EndAction(true);
        }
    }
}

