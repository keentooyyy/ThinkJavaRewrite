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
            int mask = detectionMode == DetectionMode.GenericOnly ? ~0 : interactableLayer;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(
                agent.position,
                detectionRadius.value,
                mask
            );

            GameObject closest = null;
            PickupSlot closestSlot = null;
            float closestDistance = Mathf.Infinity;

            foreach (var col in colliders)
            {
                if (!string.IsNullOrEmpty(filterTag.value) && !col.CompareTag(filterTag.value))
                {
                    continue;
                }

                var slot = col.GetComponentInParent<PickupSlot>();
                var interactable = col.GetComponentInParent<Interactable>();
                GameObject candidate = null;
                PickupSlot candidateSlot = null;

                if (detectionMode == DetectionMode.GenericOnly)
                {
                    // First, check if this collider belongs to an interactable (standalone or in slot)
                    if (interactable != null)
                    {
                        // Check if this interactable is inside a slot
                        candidateSlot = interactable.GetComponentInParent<PickupSlot>();
                        
                        // Return the interactable itself (whether it's in a slot or not)
                        // This allows picking up items directly, even if they're in slots
                        candidate = interactable.gameObject;
                    }
                    // If no interactable found on this collider, check if it's a slot with an item
                    else if (slot != null && slot.HasItem && slot.CurrentObject != null)
                    {
                        // Important: Return the item inside the slot, NOT the slot GameObject
                        // This is the key fix - the item can be picked up, not the slot
                        candidate = slot.CurrentObject;
                        candidateSlot = slot;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    // SlotsOnly mode: only detect slots
                    if (slot == null)
                    {
                        continue;
                    }
                    candidate = slot.gameObject;
                    candidateSlot = slot;
                }

                // Calculate distance to the candidate object (not the collider)
                Vector2 candidatePosition = candidate != null ? (Vector2)candidate.transform.position : col.transform.position;
                float distance = Vector2.Distance(agent.position, candidatePosition);
                
                if (distance < closestDistance)
                {
                    closest = candidate;
                    closestSlot = candidateSlot;
                    closestDistance = distance;
                }
            }

            nearbyInteractable.value = closest;
            nearbySlot.value = closestSlot;
        }

        protected override void OnStop()
        {
            nearbyInteractable.value = null;
            nearbySlot.value = null;
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

