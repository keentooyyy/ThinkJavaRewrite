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
        
        private Collider2D interactCollider;
        
        private void Awake()
        {
            // Get collider and ensure it's a trigger
            interactCollider = GetComponent<Collider2D>();
            
            if (interactCollider != null)
            {
                // Make sure at least one collider is a trigger for detection
                // (You might have multiple colliders - one solid, one trigger)
                if (!HasTriggerCollider())
                {
                    Debug.LogWarning($"Interactable on {gameObject.name} should have a trigger collider for detection!");
                }
            }
            else
            {
                Debug.LogWarning($"Interactable on {gameObject.name} needs a Collider2D component!");
            }
            
            // Validate button exists in config
            if (inputConfig != null && !IsValidButton())
            {
                Debug.LogWarning($"Interactable on {gameObject.name}: Button '{requiredButton}' not found in InputConfig!");
            }
        }
        
        private bool HasTriggerCollider()
        {
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (var col in colliders)
            {
                if (col.isTrigger)
                {
                    return true;
                }
            }
            return false;
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

