using System.Linq;
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
            
            Transform objectTransform = objectToPickUp.value.transform;
            
            // Find the carry point (search recursively in children)
            Transform carryPoint = null;
            if (agent != null && !string.IsNullOrEmpty(carryPointName.value))
            {
                carryPoint = agent.GetComponentsInChildren<Transform>()
                    .FirstOrDefault(t => t.name == carryPointName.value);
                if (carryPoint == null)
                {
                    Debug.LogWarning($"PickUpObject: Carry point '{carryPointName.value}' not found on agent '{agent.name}'. Parenting to agent root instead.");
                }
            }
            
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
            
            // Parent to carry point or player
            Transform parentTarget = carryPoint != null ? carryPoint : agent;
            objectTransform.SetParent(parentTarget);
            objectTransform.localPosition = Vector3.zero;
            objectTransform.localRotation = Quaternion.identity;
            
            // Store reference for later
            carriedObject.value = objectToPickUp.value;
            
            EndAction(true);
        }
    }
}

