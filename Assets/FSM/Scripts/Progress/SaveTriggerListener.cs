using UnityEngine;
using GameEvents;
using System.Collections.Generic;

namespace GameProgress
{
    /// <summary>
    /// Listens to multiple events and triggers TriggerSave event when any of them fire.
    /// Useful for saving after level completion, achievements, unlocks, etc.
    /// </summary>
    public class SaveTriggerListener : MonoBehaviour
    {
        [Tooltip("Events to listen for (e.g., 'ShowSuccessUI', 'LevelUnlocked', 'AchievementUnlocked')")]
        [SerializeField] private string[] listenEventNames = new string[] { "ShowSuccessUI" };

        [Tooltip("Event to trigger when any listenEventName fires (default: TriggerSave)")]
        [SerializeField] private string triggerEventName = "TriggerSave";

        private HashSet<string> subscribedEvents = new HashSet<string>();

        private void OnEnable()
        {
            if (listenEventNames == null || listenEventNames.Length == 0)
                return;

            foreach (string eventName in listenEventNames)
            {
                if (!string.IsNullOrEmpty(eventName) && !subscribedEvents.Contains(eventName))
                {
                    UIEventManager.Subscribe(eventName, OnEventFired);
                    subscribedEvents.Add(eventName);
                }
            }
        }

        private void OnDisable()
        {
            foreach (string eventName in subscribedEvents)
            {
                if (!string.IsNullOrEmpty(eventName))
                {
                    UIEventManager.Unsubscribe(eventName, OnEventFired);
                }
            }
            subscribedEvents.Clear();
        }

        private void OnEventFired()
        {
            if (!string.IsNullOrEmpty(triggerEventName))
            {
                UIEventManager.Trigger(triggerEventName);
            }
        }
    }
}

