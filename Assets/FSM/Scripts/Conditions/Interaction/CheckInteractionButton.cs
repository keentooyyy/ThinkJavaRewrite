using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameInput;
using GameInteraction;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Interaction")]
    [Description("Check if player pressed the correct button for nearby interactable")]
    public class CheckInteractionButton : CheckActionButtonPressed
    {
        [BlackboardOnly]
        [Tooltip("The nearby interactable GameObject")]
        public BBParameter<GameObject> nearbyInteractable;

        protected override string info => "Interaction Button Pressed?";

        protected override bool OnCheck()
        {
            interactable.value = nearbyInteractable.value;
            useInteractableButton = true;
            buttonName.value = string.Empty; // fallback only if interactable has none
            return base.OnCheck();
        }
    }
}
