using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Interaction")]
    [Description("Check if interactable has Pickup tag or is on Pickup layer")]
    public class CheckIsPickupable : ConditionTask
    {
        [BlackboardOnly]
        [Tooltip("The interactable GameObject to check")]
        public BBParameter<GameObject> interactable;
        
        [Tooltip("Pickup tag name to check for")]
        public BBParameter<string> pickupTag = "Pickup";
        
        [Tooltip("Pickup layer to check (0 = ignore layer check)")]
        public LayerMask pickupLayer;

        protected override string info => "Is Pickupable?";

        protected override bool OnCheck()
        {
            if (interactable.value == null)
                return false;

            GameObject obj = interactable.value;
            
            // Check tag
            bool hasPickupTag = !string.IsNullOrEmpty(pickupTag.value) && obj.CompareTag(pickupTag.value);
            
            // Check layer
            bool isOnPickupLayer = pickupLayer.value != 0 && ((1 << obj.layer) & pickupLayer.value) != 0;
            
            // Is pickupable if it has the tag OR is on the layer
            return hasPickupTag || isOnPickupLayer;
        }
    }
}

