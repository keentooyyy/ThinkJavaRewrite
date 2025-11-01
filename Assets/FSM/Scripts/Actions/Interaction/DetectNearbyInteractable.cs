using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameInteraction;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Interaction")]
    [Description("Detect nearby interactable using proximity check (no trigger needed!)")]
    public class DetectNearbyInteractable : ActionTask<Transform>
    {
        [BlackboardOnly]
        [Tooltip("Store the nearby interactable GameObject here")]
        public BBParameter<GameObject> nearbyInteractable;

        [BlackboardOnly]
        [Tooltip("Store the nearby pickup slot (if any)")]
        public BBParameter<PickupSlot> nearbySlot;

        public enum DetectionMode
        {
            GenericOnly,
            SlotsOnly
        }

        [Tooltip("Which category of interactable to detect.")]
        public DetectionMode detectionMode = DetectionMode.GenericOnly;
        
        [Tooltip("Detection radius")]
        public BBParameter<float> detectionRadius = 1.5f;
        
        [Tooltip("Layer mask for interactables")]
        public LayerMask interactableLayer;
        
        [Tooltip("Tag filter (optional, leave empty to detect all)")]
        public BBParameter<string> filterTag = "";
        
        protected override string info
        {
            get { return "Detect Nearby Interactable"; }
        }
        
        protected override void OnUpdate()
        {
            // Find nearest interactable in radius
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                agent.position,
                detectionRadius.value,
                interactableLayer
            );

            GameObject closest = null;
            PickupSlot closestSlot = null;
            float closestDistance = Mathf.Infinity;
            
            foreach (var col in colliders)
            {
                // Check if it has Interactable component
                Interactable interactable = col.GetComponent<Interactable>();
                if (interactable == null)
                    continue;

                bool isSlot = interactable.kind == InteractableKind.PickupSlot;

                if ((detectionMode == DetectionMode.GenericOnly && isSlot) ||
                    (detectionMode == DetectionMode.SlotsOnly && !isSlot))
                    continue;

                PickupSlot slot = null;
                if (detectionMode == DetectionMode.SlotsOnly)
                {
                    slot = interactable.slotReference != null ? interactable.slotReference : interactable.GetComponent<PickupSlot>();
                    if (slot == null)
                    {
                        Debug.LogWarning($"[DetectNearbyInteractable] Slot '{col.name}' is marked as PickupSlot but has no PickupSlot component.");
                        continue;
                    }
                }

                // Check tag filter if specified
                if (!string.IsNullOrEmpty(filterTag.value) && !col.CompareTag(filterTag.value))
                    continue;
                
                // Find closest
                float distance = Vector2.Distance(agent.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closest = col.gameObject;
                    closestSlot = slot;
                    closestDistance = distance;
                }
            }
            
            // Update blackboard
            nearbyInteractable.value = closest;
            if (detectionMode == DetectionMode.SlotsOnly)
            {
                nearbySlot.value = closestSlot;
                if (closestSlot != null)
                {
                    Debug.Log($"[DetectNearbyInteractable] Nearby slot set to {closestSlot.name}");
                }
            }
        }
        
        protected override void OnStop()
        {
            // Clear reference
            nearbyInteractable.value = null;
            if (detectionMode == DetectionMode.SlotsOnly)
            {
                nearbySlot.value = null;
            }
        }
        
        // Draw detection radius in editor
        public override void OnDrawGizmosSelected()
        {
            if (agent != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(agent.position, detectionRadius.value);
            }
        }
    }
}

