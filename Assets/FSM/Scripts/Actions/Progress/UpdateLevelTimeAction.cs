using NodeCanvas.Framework;
using ParadoxNotion.Design;
using GameProgress;
using GameEvents;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Progress/Levels")]
    [Description("Update level completion time. Updates bestTime if this is better.")]
    public class UpdateLevelTimeAction : ActionTask
    {
        [RequiredField]
        [Tooltip("Level ID (e.g., 'Level1')")]
        public BBParameter<string> levelId;

        [RequiredField]
        [Tooltip("Completion time in seconds")]
        public BBParameter<float> completionTime;

        [Tooltip("Optional: Event to trigger after updating (e.g., 'LevelTimeUpdated')")]
        public BBParameter<string> onUpdatedEventName;

        protected override string info => $"Update Level Time [{levelId}] = {completionTime}";

        protected override void OnExecute()
        {
            if (string.IsNullOrEmpty(levelId.value))
            {
                Debug.LogError("Level ID is empty!");
                EndAction(false);
                return;
            }

            LevelManager.UpdateLevelTime(levelId.value, completionTime.value);

            if (onUpdatedEventName != null && !string.IsNullOrEmpty(onUpdatedEventName.value))
            {
                UIEventManager.Trigger(onUpdatedEventName.value);
            }

            EndAction(true);
        }
    }
}

