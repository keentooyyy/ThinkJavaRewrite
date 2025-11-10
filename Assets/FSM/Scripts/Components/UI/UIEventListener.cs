using UnityEngine;
using GameEvents;
using System.Collections.Generic;

namespace UI
{
    /// <summary>
    /// Put this on any UI panel/GameObject.
    /// It will automatically show/hide when specific events are triggered.
    /// Can also hide other panels when showing itself (exclusive mode).
    /// </summary>
    public class UIEventListener : MonoBehaviour
    {
        [Header("Events to Show this UI")]
        [Tooltip("When any of these events fire, this GameObject will be shown")]
        public string[] showOnEvents;

        [Header("Events to Hide this UI")]
        [Tooltip("When any of these events fire, this GameObject will be hidden")]
        public string[] hideOnEvents;

        [Header("Options")]
        [Tooltip("Should this GameObject start hidden?")]
        public bool startHidden = false;

        [Tooltip("When showing this UI, hide all other UIEventListeners with 'canBeHiddenByOthers' enabled?")]
        public bool hideOthersOnShow = false;

        [Tooltip("Can other UIEventListeners hide this when they show?")]
        public bool canBeHiddenByOthers = true;

        // Static list to track all listeners
        private static List<UIEventListener> allListeners = new List<UIEventListener>();

        private void Awake()
        {
            allListeners.Add(this);

            // Subscribe to events in Awake so they work even if GameObject starts disabled
            foreach (string eventName in showOnEvents)
            {
                if (!string.IsNullOrEmpty(eventName))
                {
                    UIEventManager.Subscribe(eventName, Show);
                }
            }

            foreach (string eventName in hideOnEvents)
            {
                if (!string.IsNullOrEmpty(eventName))
                {
                    UIEventManager.Subscribe(eventName, Hide);
                }
            }
        }

        private void Start()
        {
            // Only handle initial visibility here
            if (startHidden)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            allListeners.Remove(this);

            // Unsubscribe from all events
            foreach (string eventName in showOnEvents)
            {
                if (!string.IsNullOrEmpty(eventName))
                {
                    UIEventManager.Unsubscribe(eventName, Show);
                }
            }

            foreach (string eventName in hideOnEvents)
            {
                if (!string.IsNullOrEmpty(eventName))
                {
                    UIEventManager.Unsubscribe(eventName, Hide);
                }
            }
        }

        private void Show()
        {
            // Hide others first if enabled
            if (hideOthersOnShow)
            {
                foreach (UIEventListener listener in allListeners)
                {
                    if (listener != this && listener.canBeHiddenByOthers && listener.gameObject.activeSelf)
                    {
                        listener.gameObject.SetActive(false);
                    }
                }
            }

            gameObject.SetActive(true);
        }

        private void Hide()
        {
            // Check if there's a UIScaleBounceAnimator - if so, let it handle the hiding
            var animator = GetComponent<UIScaleBounceAnimator>();
            if (animator != null)
            {
                // Animator will handle hiding with animation
                return;
            }
            
            gameObject.SetActive(false);
        }
    }
}

