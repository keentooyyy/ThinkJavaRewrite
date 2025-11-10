using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using GameEvents;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Login")]
    [Description("Logout and clear credentials")]
    public class LogoutAction : ActionTask
    {
        [Tooltip("Optional: Event to trigger after logout")]
        public BBParameter<string> onLogoutEventName;

        protected override string info => "Logout";

        protected override void OnExecute()
        {
            LoginManager.Logout();

            if (onLogoutEventName != null && !string.IsNullOrEmpty(onLogoutEventName.value))
            {
                UIEventManager.Trigger(onLogoutEventName.value);
            }

            EndAction(true);
        }
    }
}

