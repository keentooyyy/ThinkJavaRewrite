using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Interaction")]
    [Description("Pick up an object and attach it to player (carry system)")]
    public class PickUpObject : ActionTask<Transform>
    {
        [RequiredField]
        [BlackboardOnly]
        [Tooltip("The object to pick up")]
        public BBParameter<GameObject> objectToPickUp;
        
        [Tooltip("Local position offset when carrying (relative to player)")]
        public BBParameter<Vector3> holdOffset = new Vector3(0.5f, 0.5f, 0);
        
        [Tooltip("Disable object's physics when picked up?")]
        public BBParameter<bool> disablePhysics = true;
        
        [Tooltip("Disable object's colliders when picked up?")]
        public BBParameter<bool> disableColliders = true;
        
        [BlackboardOnly]
        [Tooltip("Store reference to carried object (for dropping later)")]
        public BBParameter<GameObject> carriedObject;
        
        protected override string info
        {
            get { return "Pick Up Object"; }
        }
        
        protected override void OnExecute()
        {
            if (objectToPickUp.value == null)
            {
                EndAction(false);
                return;
            }
            
            Transform objectTransform = objectToPickUp.value.transform;
            
            // Disable physics
            if (disablePhysics.value)
            {
                Rigidbody2D rb = objectToPickUp.value.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.simulated = false;
                }
            }
            
            // Disable colliders (except triggers)
            if (disableColliders.value)
            {
                Collider2D[] colliders = objectToPickUp.value.GetComponents<Collider2D>();
                foreach (var col in colliders)
                {
                    if (!col.isTrigger)
                    {
                        col.enabled = false;
                    }
                }
            }
            
            // Parent to player
            objectTransform.SetParent(agent);
            objectTransform.localPosition = holdOffset.value;
            objectTransform.localRotation = Quaternion.identity;
            
            // Store reference for later
            carriedObject.value = objectToPickUp.value;
            
            Debug.Log($"Picked up {objectToPickUp.value.name}");
            EndAction(true);
        }
    }
}

