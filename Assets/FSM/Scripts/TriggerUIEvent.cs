using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameEvents;

namespace NodeCanvas.Tasks.Actions
{
    /// <summary>
    /// Trigger a UI event by name.
    /// Useful for programmatically firing events from FSM.
    /// </summary>
    [Category("✫ Custom/UI Events")]
    [Description("Trigger a UI event by name")]
    public class TriggerUIEvent : ActionTask
    {
        [RequiredField]
        [Tooltip("The event name to trigger")]
        public BBParameter<string> eventName;

        protected override string info
        {
            get { return string.Format("Trigger '{0}' event", eventName); }
        }

        protected override void OnExecute()
        {
            if (!string.IsNullOrEmpty(eventName.value))
            {
                UIEventManager.Trigger(eventName.value);
            }
            EndAction(true);
        }
    }
}

