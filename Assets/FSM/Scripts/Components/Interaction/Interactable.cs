using UnityEngine;
using GameInput;

namespace GameInteraction
{
    public enum InteractableKind
    {
        Generic,
        PickupSlot
    }

    /// <summary>
    /// Defines what button is needed to interact with this object
    /// Attach to pickups, chests, doors, puzzle slots, etc.
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

        [Header("Interactable Kind")]
        [Tooltip("Determines how this interactable behaves in gameplay.")]
        public InteractableKind kind = InteractableKind.Generic;

        [Tooltip("Reference to the pickup slot component when Kind = PickupSlot.")]
        public PickupSlot slotReference;

        private void Awake()
        {
            if (kind == InteractableKind.PickupSlot && slotReference == null)
            {
                slotReference = GetComponent<PickupSlot>();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (kind == InteractableKind.PickupSlot && slotReference == null)
            {
                slotReference = GetComponent<PickupSlot>();
            }
        }
#endif
    }
}

