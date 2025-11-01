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
                if (!string.IsNullOrEmpty(filterTag.value) && !col.CompareTag(filterTag.value))
                {
                    continue;
                }

                PickupSlot slot = null;
                GameObject candidate = col.gameObject;
                if (detectionMode == DetectionMode.GenericOnly)
                {
                    if (col.GetComponent<Interactable>() == null)
                    {
                        continue;
                    }
                }
                else
                {
                    slot = col.GetComponent<PickupSlot>();
                    if (slot == null)
                    {
                        continue;
                    }
                    candidate = slot.gameObject;
                }

                float distance = Vector2.Distance(agent.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closest = candidate;
                    if (detectionMode == DetectionMode.SlotsOnly)
                    {
                        closestSlot = slot;
                    }
                    closestDistance = distance;
                }
            }

            nearbyInteractable.value = closest;
            nearbySlot.value = detectionMode == DetectionMode.SlotsOnly ? closestSlot : null;
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

