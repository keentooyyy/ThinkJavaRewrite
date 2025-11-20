using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using GameEvents;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Achievements")]
    [Description("Unlock an achievement. Title and description come from login JSON save data.")]
    public class UnlockAchievementAction : ActionTask
    {
        [RequiredField]
        [AchievementId]
        [Tooltip("Achievement ID - select from dropdown (populated from login JSON)")]
        public BBParameter<string> achievementId;

        [Tooltip("Optional: Event to trigger after unlocking")]
        public BBParameter<string> onUnlockedEventName;

        protected override string info => $"Unlock Achievement [{achievementId}]";

        protected override void OnExecute()
        {
            if (string.IsNullOrEmpty(achievementId.value))
            {
                Debug.LogError("Achievement ID is empty!");
                EndAction(false);
                return;
            }

            AchievementManager.UnlockAchievement(achievementId.value);

            if (onUnlockedEventName != null && !string.IsNullOrEmpty(onUnlockedEventName.value))
            {
                UIEventManager.Trigger(onUnlockedEventName.value);
            }

            EndAction(true);
        }
    }
}

