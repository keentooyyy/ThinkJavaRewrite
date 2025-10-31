using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Interaction")]
    [Description("Drop a carried object (detach from player and re-enable physics)")]
    public class DropObject : ActionTask<Transform>
    {
        [BlackboardOnly]
        [Tooltip("The carried object to drop")]
        public BBParameter<GameObject> carriedObject;
        
        [Tooltip("Offset from player when dropped")]
        public BBParameter<Vector3> dropOffset = new Vector3(1f, 0, 0);
        
        [Tooltip("Apply throw force?")]
        public BBParameter<bool> applyThrowForce = false;
        
        [Tooltip("Throw force (horizontal, vertical)")]
        public BBParameter<Vector2> throwForce = new Vector2(3f, 2f);
        
        [Tooltip("Name of parent GameObject to organize pickups (e.g., 'Pickups & Interactables Canvas')")]
        public BBParameter<string> pickupsParentName = "Pickups & Interactables Canvas";
        
        protected override string info
        {
            get { return "Drop Object"; }
        }
        
        protected override void OnExecute()
        {
            if (carriedObject.value == null)
            {
                EndAction(false);
                return;
            }
            
            Transform objectTransform = carriedObject.value.transform;
            
            // Store world position before deparenting
            Vector3 worldPos = objectTransform.position;
            
            // Find the pickups parent container
            GameObject pickupsParent = null;
            if (!string.IsNullOrEmpty(pickupsParentName.value))
            {
                pickupsParent = GameObject.Find(pickupsParentName.value);
                
                if (pickupsParent == null)
                {
                    Debug.LogWarning($"DropObject: Parent '{pickupsParentName.value}' not found! Dropping to scene root.");
                }
            }
            
            // Detach from player and set to pickups parent
            if (pickupsParent != null)
            {
                objectTransform.SetParent(pickupsParent.transform, true); // Keep world position
            }
            else
            {
                objectTransform.SetParent(null); // Drop to scene root
            }
            
            objectTransform.position = worldPos + dropOffset.value;
            
            // Re-enable physics
            Rigidbody2D rb = carriedObject.value.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = true;
                
                // Apply throw force if enabled
                if (applyThrowForce.value)
                {
                    // Throw in direction player is facing
                    float direction = Mathf.Sign(agent.localScale.x);
                    rb.velocity = new Vector2(throwForce.value.x * direction, throwForce.value.y);
                }
            }
            
            // Re-enable colliders
            Collider2D[] colliders = carriedObject.value.GetComponents<Collider2D>();
            foreach (var col in colliders)
            {
                col.enabled = true;
            }
            
            // Clear reference
            carriedObject.value = null;
            
            EndAction(true);
        }
    }
}

