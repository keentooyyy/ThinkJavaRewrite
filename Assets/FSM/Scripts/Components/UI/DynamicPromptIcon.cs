using UnityEngine;
using UnityEngine.UI;
using GameEvents;

namespace UI
{
    /// <summary>
    /// Single image that changes sprite based on UI events
    /// Shows E icon for ActionA, F icon for ActionB, etc.
    /// Attach to an Image component in your Canvas
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class DynamicPromptIcon : MonoBehaviour
    {
        [Header("Button Icons")]
        [Tooltip("Sprite to show for ActionA (E key)")]
        public Sprite iconActionA;
        
        [Tooltip("Sprite to show for ActionB (F key)")]
        public Sprite iconActionB;
        
        // Add more button icons here if needed in the future
        
        private Image image;
        
        private void Awake()
        {
            image = GetComponent<Image>();
            
            // Subscribe to UI events
            UIEventManager.Subscribe("ShowPromptActionA", ShowActionA);
            UIEventManager.Subscribe("ShowPromptActionB", ShowActionB);
            UIEventManager.Subscribe("HidePrompt", Hide);
            
            // Start hidden
            gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from all events
            UIEventManager.Unsubscribe("ShowPromptActionA", ShowActionA);
            UIEventManager.Unsubscribe("ShowPromptActionB", ShowActionB);
            UIEventManager.Unsubscribe("HidePrompt", Hide);
        }
        
        private void ShowActionA()
        {
            if (iconActionA == null)
            {
                return;
            }

            image.sprite = iconActionA;
            gameObject.SetActive(true);
        }
        
        private void ShowActionB()
        {
            if (iconActionB == null)
            {
                return;
            }

            image.sprite = iconActionB;
            gameObject.SetActive(true);
        }
        
        private void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}

