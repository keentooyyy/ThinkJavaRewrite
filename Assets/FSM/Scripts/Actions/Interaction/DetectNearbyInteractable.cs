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
            float closestDistance = Mathf.Infinity;
            
            foreach (var col in colliders)
            {
                // Check if it has Interactable component
                Interactable interactable = col.GetComponent<Interactable>();
                if (interactable == null)
                    continue;
                
                // Check tag filter if specified
                if (!string.IsNullOrEmpty(filterTag.value) && !col.CompareTag(filterTag.value))
                    continue;
                
                // Find closest
                float distance = Vector2.Distance(agent.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closest = col.gameObject;
                    closestDistance = distance;
                }
            }
            
            // Update blackboard
            nearbyInteractable.value = closest;
        }
        
        protected override void OnStop()
        {
            // Clear reference
            nearbyInteractable.value = null;
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

