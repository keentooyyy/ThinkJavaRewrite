using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameEvents;

namespace NodeCanvas.Tasks.Actions
{
    /// <summary>
    /// Waits for a UI event by name (event-driven, not scene-specific).
    /// Any button with UIEventButton component can trigger this.
    /// </summary>
    [Category("✫ Custom/UI Events")]
    [Description("Wait for a UI event to be triggered by name (e.g. 'PlayButton')")]
    public class WaitUIEvent : ActionTask
    {
        [RequiredField]
        [Tooltip("The event name to listen for (must match UIEventButton's eventName)")]
        public BBParameter<string> eventName;
        
        private bool wasTriggered = false;

        protected override string info
        {
            get { return string.Format("Wait for '{0}' event", eventName); }
        }

        protected override string OnInit()
        {
            if (string.IsNullOrEmpty(eventName.value))
            {
                return "Event name cannot be empty!";
            }
            return null;
        }

        protected override void OnExecute()
        {
            wasTriggered = false;
            UIEventManager.Subscribe(eventName.value, OnEventTriggered);
        }

        protected override void OnUpdate()
        {
            if (wasTriggered)
            {
                EndAction(true);
            }
        }

        protected override void OnStop()
        {
            UIEventManager.Unsubscribe(eventName.value, OnEventTriggered);
        }

        private void OnEventTriggered()
        {
            wasTriggered = true;
        }
    }
}

