using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("■ Custom/Progress/Achievements")]
    [Description("Check if an achievement is unlocked")]
    public class CheckAchievementUnlocked : ConditionTask
    {
        [RequiredField]
        [Tooltip("Achievement ID to check")]
        public BBParameter<string> achievementId;

        protected override string info => $"Achievement [{achievementId}] Unlocked?";

        protected override bool OnCheck()
        {
            if (string.IsNullOrEmpty(achievementId.value))
            {
                return false;
            }

            return AchievementManager.IsUnlocked(achievementId.value);
        }
    }
}

