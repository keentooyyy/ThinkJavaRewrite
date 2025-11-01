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
                    if (interactable != null)
                    {
                        candidate = interactable.gameObject;
                        candidateSlot = interactable.GetComponentInParent<PickupSlot>();
                    }
                    else if (slot != null && slot.HasItem && slot.CurrentObject != null)
                    {
                        candidate = slot.gameObject;
                        candidateSlot = slot;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    if (slot == null)
                    {
                        continue;
                    }
                    candidate = slot.gameObject;
                    candidateSlot = slot;
                }

                float distance = Vector2.Distance(agent.position, col.transform.position);
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

