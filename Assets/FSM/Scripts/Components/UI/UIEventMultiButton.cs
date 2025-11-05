using UnityEngine;
using UnityEngine.UI;
using GameEvents;

namespace UI
{
    /// <summary>
    /// Single component to trigger multiple UI events from one Button click.
    /// Keeps event fan-out in one place instead of stacking multiple UIEventButton instances.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIEventMultiButton : MonoBehaviour
    {
        [Tooltip("List of UI events to trigger when this button is clicked.")]
        public string[] eventNames;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClick);
            }
        }

        private void OnClick()
        {
            if (eventNames == null)
            {
                return;
            }

            for (int i = 0; i < eventNames.Length; i++)
            {
                var evt = eventNames[i];
                if (!string.IsNullOrEmpty(evt))
                {
                    UIEventManager.Trigger(evt);
                }
            }
        }
    }
}

