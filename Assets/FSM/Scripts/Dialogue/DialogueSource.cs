using UnityEngine;
using Dialogue;

namespace DialogueRuntime
{
    /// <summary>
    /// Attach alongside an Interactable to link a DialogueSequence with it.
    /// </summary>
    [RequireComponent(typeof(GameInteraction.Interactable))]
    public class DialogueSource : MonoBehaviour
    {
        [SerializeField] private DialogueSequence sequence;

        [Header("Auto Trigger Settings")]
        [Tooltip("Automatically trigger dialogue when player enters trigger area")]
        [SerializeField] private bool autoTrigger = false;
        
        [Tooltip("Trigger only once (won't trigger again after first time)")]
        [SerializeField] private bool triggerOnce = false;
        
        [Tooltip("Detection radius for auto trigger (only used if no trigger collider)")]
        [SerializeField] private float autoTriggerRadius = 2f;

        private bool hasTriggered = false;

        public DialogueSequence Sequence => sequence;
        public bool HasDialogue => sequence != null;
        public bool AutoTrigger => autoTrigger;
        public bool TriggerOnce => triggerOnce;
        public bool HasTriggered => hasTriggered;
        public float AutoTriggerRadius => autoTriggerRadius;

        /// <summary>
        /// Mark this dialogue as triggered (used for triggerOnce functionality)
        /// </summary>
        public void MarkTriggered()
        {
            hasTriggered = true;
        }

        /// <summary>
        /// Reset the triggered state (allows triggering again)
        /// </summary>
        public void ResetTriggered()
        {
            hasTriggered = false;
        }
    }
}

