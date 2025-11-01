using System.Linq;
using GameInteraction;
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
        
        [Tooltip("Name of the carry point under the player")]
        public BBParameter<string> carryPointName = "CarryPoint";
        
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
            
            GameObject pickupTarget = objectToPickUp.value;

            // If we're targeting a slot, attempt to retrieve its current item first.
            var slot = pickupTarget.GetComponent<PickupSlot>();
            if (slot != null)
            {
                var retrieved = slot.RetrieveCurrent();
                if (retrieved == null)
                {
                    EndAction(false);
                    return;
                }

                objectToPickUp.value = retrieved;
                pickupTarget = retrieved;
            }

            Transform objectTransform = pickupTarget.transform;
            
            // Find the carry point (search recursively in children)
            Transform carryPoint = null;
            if (agent != null && !string.IsNullOrEmpty(carryPointName.value))
            {
                carryPoint = agent.GetComponentsInChildren<Transform>()
                    .FirstOrDefault(t => t.name == carryPointName.value);
                // If we can't find the carry point, fall back to the agent root.
            }
            
            // Disable physics
            if (disablePhysics.value)
            {
                Rigidbody2D rb = pickupTarget.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.simulated = false;
                }
            }
            
            // Disable colliders (except triggers)
            if (disableColliders.value)
            {
                Collider2D[] colliders = pickupTarget.GetComponents<Collider2D>();
                foreach (var col in colliders)
                {
                    if (!col.isTrigger)
                    {
                        col.enabled = false;
                    }
                }
            }
            
            // Parent to carry point or player
            Transform parentTarget = carryPoint != null ? carryPoint : agent;
            Vector3 worldScale = TransformUtilities.NormalizeWorldScale(objectTransform.lossyScale);
            Quaternion worldRotation = objectTransform.rotation;

            objectTransform.SetParent(parentTarget, true);
            TransformUtilities.SetWorldScale(objectTransform, worldScale);
            objectTransform.localPosition = Vector3.zero;
            objectTransform.localRotation = Quaternion.identity;
            objectTransform.rotation = worldRotation;

            MaintainWorldScale.Attach(pickupTarget, worldScale);
            
            // Store reference for later
            carriedObject.value = pickupTarget;
            
            EndAction(true);
        }
    }
}

