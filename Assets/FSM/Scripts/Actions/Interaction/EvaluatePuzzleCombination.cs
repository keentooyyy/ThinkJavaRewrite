using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameInteraction;
using GameEvents;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Interaction")]
    [Description("Evaluates if the puzzle combination is correct. Rejects if wrong, triggers success if correct.")]
    public class EvaluatePuzzleCombination : ActionTask
    {
        [RequiredField]
        [Tooltip("Reference to the PickupPuzzleFSMController")]
        public BBParameter<PickupPuzzleFSMController> puzzleController;

        protected override string info => "Evaluate Puzzle Combination";

        protected override void OnExecute()
        {
            var controller = puzzleController.value;
            if (controller == null)
            {
                EndAction(false);
                return;
            }

            var dataSlot = controller.DataTypeSlot;
            var variableSlot = controller.VariableSlot;
            var config = controller.PuzzleConfig;

            if (dataSlot == null || variableSlot == null || config == null)
            {
                EndAction(false);
                return;
            }

            if (!dataSlot.HasItem || !variableSlot.HasItem)
            {
                EndAction(false);
                return;
            }

            // Try to get the required pair from config
            if (!config.TryGetPair(dataSlot.SlotDefinition, variableSlot.SlotDefinition, out var pair))
            {
                RejectSlots(dataSlot, variableSlot);
                EndAction(false);
                return;
            }

            // Check if the placed items match the requirements
            var actualType = dataSlot.CurrentDataType;
            var actualVariable = variableSlot.CurrentVariableNormalized;
            var expectedVariable = PickupMetadata.NormalizeIdentifier(pair.requiredVariableName);

            bool typeMatches = actualType == pair.requiredType;
            bool variableMatches = actualVariable == expectedVariable;

            if (!typeMatches || !variableMatches)
            {
                RejectSlots(dataSlot, variableSlot);
                EndAction(false);
                return;
            }

            // Success: trigger configured UI event
            if (!string.IsNullOrEmpty(controller.PuzzleSuccessEvent))
            {
                UIEventManager.Trigger(controller.PuzzleSuccessEvent);
            }

            EndAction(true);
        }

        private void RejectSlots(PickupSlot dataSlot, PickupSlot variableSlot)
        {
            if (dataSlot != null)
            {
                dataSlot.RejectCurrent();
            }

            if (variableSlot != null)
            {
                variableSlot.RejectCurrent();
            }
        }
    }
}

