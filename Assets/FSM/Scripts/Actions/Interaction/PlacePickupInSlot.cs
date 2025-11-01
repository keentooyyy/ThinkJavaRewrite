using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameInteraction;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Interaction")]
    [Description("Attempt to place the carried object into the targeted puzzle slot.")]
    public class PlacePickupInSlot : ActionTask
    {
        [BlackboardOnly]
        [Tooltip("Reference to the carried object (set by PickUpObject).")]
        public BBParameter<GameObject> carriedObject;

        [BlackboardOnly]
        [Tooltip("Target slot to try placing the carried object into.")]
        public BBParameter<PickupSlot> targetSlot;

        protected override string info => "Place Pickup In Slot";

        protected override void OnExecute()
        {
            var carried = carriedObject.value;
            var slot = targetSlot.value;

            if (carried == null || slot == null)
            {
                Debug.LogWarning("[PlacePickupInSlot] Missing carried object or slot.");
                EndAction(false);
                return;
            }

            var interactable = carried.GetComponent<Interactable>();
            if (interactable == null)
            {
                Debug.LogWarning("[PlacePickupInSlot] Carried object has no Interactable component.");
                EndAction(false);
                return;
            }

            if (slot.TryPlace(interactable))
            {
                Debug.Log("[PlacePickupInSlot] Placement succeeded.");
                carriedObject.value = null;
                EndAction(true);
            }
            else
            {
                Debug.LogWarning("[PlacePickupInSlot] Slot rejected the carried object.");
                carriedObject.value = null;
                EndAction(false);
            }
        }
    }
}


