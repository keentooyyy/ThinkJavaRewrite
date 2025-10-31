using UnityEngine;
using GameInput;

namespace GameInteraction
{
    /// <summary>
    /// Defines what button is needed to interact with this object
    /// Attach to pickups, chests, doors, etc.
    /// Uses trigger collider for detection
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Interactable : MonoBehaviour
    {
        [Header("Input Config Reference")]
        [Tooltip("Reference to your InputConfig asset (e.g., MainInputConfig)")]
        public InputConfig inputConfig;
        
        [Header("Interaction Settings")]
        [ButtonName("inputConfig")]
        [Tooltip("Which button activates this? (Choose from InputConfig dropdown)")]
        public string requiredButton = "ActionA";
        
        private void Awake()
        {
            // Validate collider exists
            Collider2D interactCollider = GetComponent<Collider2D>();
            
            if (interactCollider == null)
            {
                Debug.LogWarning($"Interactable on {gameObject.name} needs a Collider2D component!");
            }
            else if (!interactCollider.isTrigger)
            {
                Debug.LogWarning($"Interactable on {gameObject.name} should have its collider set to 'Is Trigger'!");
            }
            
            // Validate button exists in config
            if (inputConfig != null && !IsValidButton())
            {
                Debug.LogWarning($"Interactable on {gameObject.name}: Button '{requiredButton}' not found in InputConfig!");
            }
        }
        
        private bool IsValidButton()
        {
            if (inputConfig == null || inputConfig.keyboardBindings == null)
                return false;
            
            foreach (var binding in inputConfig.keyboardBindings)
            {
                if (binding.buttonName == requiredButton)
                    return true;
            }
            return false;
        }
    }
}

