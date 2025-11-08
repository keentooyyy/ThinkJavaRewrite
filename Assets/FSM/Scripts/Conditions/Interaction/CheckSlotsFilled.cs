using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameInteraction;

namespace NodeCanvas.Tasks.Conditions
{
    [Category("✫ Custom/Interaction")]
    [Description("Check if both puzzle slots are filled")]
    public class CheckSlotsFilled : ConditionTask
    {
        [RequiredField]
        [Tooltip("Reference to the PickupPuzzleFSMController")]
        public BBParameter<PickupPuzzleFSMController> puzzleController;

        protected override string info => "Both Slots Filled?";

        protected override bool OnCheck()
        {
            if (puzzleController.value == null)
                return false;

            var dataSlot = puzzleController.value.DataTypeSlot;
            var variableSlot = puzzleController.value.VariableSlot;

            if (dataSlot == null || variableSlot == null)
                return false;

            return dataSlot.HasItem && variableSlot.HasItem;
        }
    }
}

