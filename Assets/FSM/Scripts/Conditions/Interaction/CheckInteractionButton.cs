using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameInput;
using GameInteraction;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Interaction")]
    [Description("Check if player pressed the correct button for nearby interactable")]
    public class CheckInteractionButton : ConditionTask
    {
        [BlackboardOnly]
        [Tooltip("The nearby interactable GameObject")]
        public BBParameter<GameObject> nearbyInteractable;
        
        protected override string info
        {
            get { return "Interaction Button Pressed?"; }
        }
        
        protected override bool OnCheck()
        {
            // Check if we have an interactable nearby
            if (nearbyInteractable.value == null)
                return false;
            
            // Get the Interactable component
            Interactable interactable = nearbyInteractable.value.GetComponent<Interactable>();
            if (interactable == null)
                return false;
            
            // Check if the required button was pressed
            string requiredButton = interactable.requiredButton;
            
            if (string.IsNullOrEmpty(requiredButton))
                return false;
            
            // Check if button was pressed this frame
            return InputManager.GetButtonDown(requiredButton);
        }
    }
}

