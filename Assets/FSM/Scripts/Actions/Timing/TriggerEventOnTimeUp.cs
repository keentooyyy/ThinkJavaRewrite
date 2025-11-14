using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameState;
using GameEvents;

namespace NodeCanvas.Tasks.Actions
{
    [Category("■ Custom/Timing")]
    [Description("Monitor a remaining-time variable and send an event (plus optional UI message) when it hits zero.")]
    public class TriggerEventOnTimeUp : ActionTask
    {
        [RequiredField]
        [BlackboardOnly]
        [Tooltip("Blackboard variable that stores remaining seconds (usually from ReadOwnerTime).")]
        public BBParameter<float> remainingSeconds;

        [Tooltip("Name of the event to send when time reaches zero.")]
        public BBParameter<string> eventName = "GameoverEvent";

        [Tooltip("Message to write when time is up.")]
        public BBParameter<string> message = "Time's up!";

        [BlackboardOnly]
        [Tooltip("Optional blackboard output for the message.")]
        public BBParameter<string> outMessage;

        [Tooltip("Optional text GameObject whose TMP_Text or Text component gets updated with the message.")]
        public BBParameter<GameObject> textObject;

        [Tooltip("Continuously check every frame while the state stays active.")]
        public BBParameter<bool> continuous = true;

        [Tooltip("Skip updates while gameplay is frozen.")]
        public BBParameter<bool> respectFreeze = true;

        private bool eventSent;
        private TMP_Text cachedTMPText;
        private Text cachedUIText;

        protected override string info => "Trigger Event On Time Up";

        protected override void OnExecute()
        {
            eventSent = false;
            cachedTMPText = null;
            cachedUIText = null;

            Evaluate();

            if (!continuous.value)
            {
                EndAction(true);
            }
        }

        protected override void OnUpdate()
        {
            if (!continuous.value || eventSent)
            {
                return;
            }

            if (respectFreeze.value && !GameFreezeManager.AllowsGameplayUpdate)
            {
                return;
            }

            Evaluate();
        }

        private void Evaluate()
        {
            if (eventSent)
            {
                return;
            }

            float remaining = remainingSeconds != null ? remainingSeconds.value : float.PositiveInfinity;
            if (remaining > 0f)
            {
                return;
            }

            eventSent = true;
            ApplyMessage();

            if (eventName != null && !string.IsNullOrEmpty(eventName.value))
            {
                UIEventManager.Trigger(eventName.value);
                // SendEvent(eventName.value);
            }
        }

        private void ApplyMessage()
        {
            string resolvedMessage = ResolveMessage();

            if (outMessage != null)
            {
                outMessage.value = resolvedMessage;
            }

            if (textObject == null || textObject.value == null)
            {
                return;
            }

            CacheTextComponents();

            if (cachedTMPText != null)
            {
                cachedTMPText.text = resolvedMessage;
            }
            else if (cachedUIText != null)
            {
                cachedUIText.text = resolvedMessage;
            }
        }

        private string ResolveMessage()
        {
            string resolvedMessage = message != null ? message.value : string.Empty;
            if (string.IsNullOrEmpty(resolvedMessage))
            {
                resolvedMessage = "Time's up!";
            }

            return resolvedMessage;
        }

        private void CacheTextComponents()
        {
            if (textObject == null || textObject.value == null)
            {
                cachedTMPText = null;
                cachedUIText = null;
                return;
            }

            if (cachedTMPText == null && cachedUIText == null)
            {
                cachedTMPText = textObject.value.GetComponent<TMP_Text>();
                if (cachedTMPText == null)
                {
                    cachedUIText = textObject.value.GetComponent<Text>();
                }
            }
        }
    }
}

