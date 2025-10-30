using UnityEngine;
using UnityEngine.UI;
using GameEvents;

namespace UI
{
    /// <summary>
    /// Add this to any button in your scene. 
    /// Instead of wiring up specific actions, just give it an event name.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIEventButton : MonoBehaviour
    {
        [Tooltip("Name of the event to trigger when clicked (e.g. 'PlayButton', 'PauseButton')")]
        public string eventName;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            if (!string.IsNullOrEmpty(eventName))
            {
                UIEventManager.Trigger(eventName);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClick);
            }
        }
    }
}

