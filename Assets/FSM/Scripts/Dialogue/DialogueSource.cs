using UnityEngine;
using Dialogue;

namespace DialogueRuntime
{
    /// <summary>
    /// Provides a dialogue sequence reference that can be triggered via interaction or automatically.
    /// When AutoTrigger is enabled the helper component (AutoDialogueTrigger) is injected automatically.
    /// </summary>
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
        [SerializeField, HideInInspector] private AutoDialogueTrigger autoTriggerBehaviour;

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

        private void Reset()
        {
            SyncAutoTriggerBehaviour();
        }

        private void Awake()
        {
            SyncAutoTriggerBehaviour();
        }

        private void OnValidate()
        {
            SyncAutoTriggerBehaviour();
        }

        private void SyncAutoTriggerBehaviour()
        {
            if (autoTriggerBehaviour != null && autoTriggerBehaviour.gameObject != gameObject)
            {
                autoTriggerBehaviour = null;
            }

            if (autoTriggerBehaviour == null)
            {
                autoTriggerBehaviour = GetComponent<AutoDialogueTrigger>();
            }

            if (!autoTrigger)
            {
                autoTriggerBehaviour?.SyncWithSource(this);
                return;
            }

            if (autoTriggerBehaviour == null)
            {
                autoTriggerBehaviour = gameObject.AddComponent<AutoDialogueTrigger>();
            }

            autoTriggerBehaviour.SyncWithSource(this);
        }
    }
}
