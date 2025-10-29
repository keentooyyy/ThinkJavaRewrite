using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameEvents
{
    /// <summary>
    /// Simple static event manager for UI button clicks.
    /// Use string-based event names to decouple from scene-specific references.
    /// </summary>
    public static class UIEventManager
    {
        private static Dictionary<string, Action> events = new Dictionary<string, Action>();

        /// <summary>
        /// Subscribe to a UI event by name
        /// </summary>
        public static void Subscribe(string eventName, Action callback)
        {
            if (!events.ContainsKey(eventName))
            {
                events[eventName] = null;
            }
            events[eventName] += callback;
        }

        /// <summary>
        /// Unsubscribe from a UI event
        /// </summary>
        public static void Unsubscribe(string eventName, Action callback)
        {
            if (events.ContainsKey(eventName))
            {
                events[eventName] -= callback;
            }
        }

        /// <summary>
        /// Trigger a UI event by name
        /// </summary>
        public static void Trigger(string eventName)
        {
            if (events.ContainsKey(eventName) && events[eventName] != null)
            {
                events[eventName].Invoke();
            }
        }

        /// <summary>
        /// Clear all events (call on scene unload if needed)
        /// </summary>
        public static void Clear()
        {
            events.Clear();
        }
    }
}

