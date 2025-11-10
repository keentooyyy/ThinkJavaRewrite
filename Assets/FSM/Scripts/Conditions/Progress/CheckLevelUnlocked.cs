using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("■ Custom/Progress/Levels")]
    [Description("Check if a level is unlocked")]
    public class CheckLevelUnlocked : ConditionTask
    {
        [RequiredField]
        [Tooltip("Level ID to check")]
        public BBParameter<string> levelId;

        protected override string info => $"Level [{levelId}] Unlocked?";

        protected override bool OnCheck()
        {
            if (string.IsNullOrEmpty(levelId.value))
            {
                return false;
            }

            return LevelManager.IsUnlocked(levelId.value);
        }
    }
}

