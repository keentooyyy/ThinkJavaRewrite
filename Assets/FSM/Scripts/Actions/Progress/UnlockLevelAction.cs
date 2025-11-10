using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using GameEvents;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Levels")]
    [Description("Unlock a level")]
    public class UnlockLevelAction : ActionTask
    {
        [RequiredField]
        [Tooltip("Level ID (e.g., 'Level1')")]
        public BBParameter<string> levelId;

        [Tooltip("Optional: Event to trigger after unlocking")]
        public BBParameter<string> onUnlockedEventName;

        protected override string info => $"Unlock Level [{levelId}]";

        protected override void OnExecute()
        {
            if (string.IsNullOrEmpty(levelId.value))
            {
                Debug.LogError("Level ID is empty!");
                EndAction(false);
                return;
            }

            LevelManager.UnlockLevel(levelId.value);

            if (onUnlockedEventName != null && !string.IsNullOrEmpty(onUnlockedEventName.value))
            {
                UIEventManager.Trigger(onUnlockedEventName.value);
            }

            EndAction(true);
        }
    }
}

