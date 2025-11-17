using UnityEngine;
using DialogueRuntime;

namespace DialogueRuntime
{
    /// <summary>
    /// Automatically triggers dialogue when player enters the trigger area.
    /// Works with DialogueSource component. Can use trigger collider or proximity check.
    /// </summary>
    [RequireComponent(typeof(DialogueSource))]
    public class AutoDialogueTrigger : MonoBehaviour
    {
        [Header("Trigger Settings")]
        [Tooltip("Use trigger collider (OnTriggerEnter2D) or proximity check (Update)")]
        [SerializeField] private bool useTriggerCollider = true;
        
        [Tooltip("Detection radius for proximity check (only used if useTriggerCollider is false)")]
        [SerializeField] private float proximityRadius = 2f;
        
        [Tooltip("Layer mask for player detection")]
        [SerializeField] private LayerMask playerLayer;
        
        [Tooltip("Player tag to detect")]
        [SerializeField] private string playerTag = "Player";

        [Header("Behaviour")]
        [Tooltip("Check every frame when using proximity mode")]
        [SerializeField] private float proximityCheckInterval = 0.1f;

        private DialogueSource dialogueSource;
        private Collider2D triggerCollider;
        private float lastProximityCheck = 0f;
        private bool hasTriggeredThisSession = false;

        private void Awake()
        {
            dialogueSource = GetComponent<DialogueSource>();
            if (dialogueSource == null)
            {
                Debug.LogError($"AutoDialogueTrigger on {gameObject.name} requires a DialogueSource component!");
                enabled = false;
                return;
            }

            // Check if auto trigger is enabled
            if (!dialogueSource.AutoTrigger)
            {
                enabled = false;
                return;
            }

            triggerCollider = GetComponent<Collider2D>();
            
            if (useTriggerCollider)
            {
                // Ensure collider is set up as trigger
                if (triggerCollider == null)
                {
                    Debug.LogWarning($"AutoDialogueTrigger on {gameObject.name} is set to use trigger collider but no Collider2D found. Adding BoxCollider2D.");
                    triggerCollider = gameObject.AddComponent<BoxCollider2D>();
                }
                
                triggerCollider.isTrigger = true;
            }
        }

        private void Update()
        {
            // Only use proximity check if not using trigger collider
            if (useTriggerCollider)
                return;

            // Throttle proximity checks
            if (Time.time - lastProximityCheck < proximityCheckInterval)
                return;

            lastProximityCheck = Time.time;

            // Check if dialogue should trigger
            if (ShouldTrigger())
            {
                CheckAndTriggerDialogue();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!useTriggerCollider)
                return;

            if (IsPlayer(collision))
            {
                CheckAndTriggerDialogue();
            }
        }

        private bool ShouldTrigger()
        {
            if (dialogueSource == null || !dialogueSource.HasDialogue)
                return false;

            if (dialogueSource.TriggerOnce && dialogueSource.HasTriggered)
                return false;

            if (dialogueSource.TriggerOnce && hasTriggeredThisSession)
                return false;

            return true;
        }

        private void CheckAndTriggerDialogue()
        {
            if (!ShouldTrigger())
                return;

            // Check if dialogue system is already active
            if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsActive)
                return;

            // Trigger the dialogue
            DialogueSystem.Instance?.BeginDialogue(dialogueSource.Sequence);
            
            // Mark as triggered
            dialogueSource.MarkTriggered();
            hasTriggeredThisSession = true;
        }

        private bool IsPlayer(Collider2D collision)
        {
            if (collision == null)
                return false;

            // Check tag
            if (!string.IsNullOrEmpty(playerTag) && collision.CompareTag(playerTag))
                return true;

            // Check layer
            if (playerLayer != 0 && ((1 << collision.gameObject.layer) & playerLayer) != 0)
                return true;

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            if (!useTriggerCollider && dialogueSource != null && dialogueSource.AutoTrigger)
            {
                Gizmos.color = Color.yellow;
                float radius = proximityRadius > 0 ? proximityRadius : dialogueSource.AutoTriggerRadius;
                Gizmos.DrawWireSphere(transform.position, radius);
            }
        }
    }
}

