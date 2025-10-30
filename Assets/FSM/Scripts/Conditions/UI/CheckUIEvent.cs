using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameEvents;

namespace NodeCanvas.Tasks.Conditions
{
    /// <summary>
    /// Check if a UI event was triggered.
    /// Use this in FSM transitions to branch based on button clicks.
    /// Cleaner than parallel WaitUIEvent states.
    /// </summary>
    [Category("✫ Custom/UI Events")]
    [Description("Check if a UI event was triggered (for FSM transitions)")]
    public class CheckUIEvent : ConditionTask
    {
        [RequiredField]
        [Tooltip("The event name to check for (e.g. 'PlayButton', 'SettingsButton')")]
        public BBParameter<string> eventName;
        
        private bool wasTriggered = false;

        protected override string info
        {
            get { return string.Format("Event '{0}' fired?", eventName); }
        }

        protected override void OnEnable()
        {
            wasTriggered = false;
            if (!string.IsNullOrEmpty(eventName.value))
            {
                UIEventManager.Subscribe(eventName.value, OnEventTriggered);
            }
        }

        protected override void OnDisable()
        {
            if (!string.IsNullOrEmpty(eventName.value))
            {
                UIEventManager.Unsubscribe(eventName.value, OnEventTriggered);
            }
        }

        protected override bool OnCheck()
        {
            if (wasTriggered)
            {
                wasTriggered = false; // Reset for next check
                return true;
            }
            return false;
        }

        private void OnEventTriggered()
        {
            wasTriggered = true;
        }
    }
}

