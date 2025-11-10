using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using GameEvents;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Achievements")]
    [Description("Unlock an achievement. Optionally set title and description if creating new achievement.")]
    public class UnlockAchievementAction : ActionTask
    {
        [RequiredField]
        [Tooltip("Achievement ID (e.g., 'ach_001')")]
        public BBParameter<string> achievementId;

        [Tooltip("Optional: Title for the achievement (only used if achievement doesn't exist)")]
        public BBParameter<string> title;

        [Tooltip("Optional: Achievement description (only used if achievement doesn't exist)")]
        public BBParameter<string> achievementDescription;

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

            string titleVal = title != null && !string.IsNullOrEmpty(title.value) ? title.value : null;
            string descVal = achievementDescription != null && !string.IsNullOrEmpty(achievementDescription.value) ? achievementDescription.value : null;

            AchievementManager.UnlockAchievement(achievementId.value, titleVal, descVal);

            if (onUnlockedEventName != null && !string.IsNullOrEmpty(onUnlockedEventName.value))
            {
                UIEventManager.Trigger(onUnlockedEventName.value);
            }

            EndAction(true);
        }
    }
}

