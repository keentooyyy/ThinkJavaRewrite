using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameInteraction;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Interaction")]
    [Description("Check if any puzzle slot pair is filled (for independent pair evaluation)")]
    public class CheckSlotsFilled : ConditionTask
    {
        [Tooltip("Reference to the PickupPuzzleFSMController (auto-resolves from agent if not set)")]
        public BBParameter<PickupPuzzleFSMController> puzzleController;

        protected override string info => "Any Slot Pair Filled?";

        protected override bool OnCheck()
        {
            // Auto-resolve controller from agent (the FSM owner) if blackboard variable is not set
            var controller = puzzleController.value;
            if (controller == null && agent != null)
            {
                controller = agent as PickupPuzzleFSMController ?? agent.GetComponent<PickupPuzzleFSMController>();
            }
            
            if (controller == null)
                return false;

            return controller.HasAnySlotPairFilled();
        }
    }
}

