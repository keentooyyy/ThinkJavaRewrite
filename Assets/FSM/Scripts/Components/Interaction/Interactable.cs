using UnityEngine;
using GameInput;

namespace GameInteraction
{
    /// <summary>
    /// Defines what button is needed to interact with this object.
    /// Attach to pickups, chests, doors, etc.
    /// Uses trigger collider for detection.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Interactable : MonoBehaviour, IActionButtonProvider
    {
        [Header("Input Config Reference")]
        [Tooltip("Reference to your InputConfig asset (e.g., MainInputConfig)")]
        public InputConfig inputConfig;

        [Header("Interaction Settings")]
        [ButtonName("inputConfig")]
        [Tooltip("Which button activates this? (Choose from InputConfig dropdown)")]
        public string requiredButton = "ActionA";

        public string RequiredButton => requiredButton;
    }
}

