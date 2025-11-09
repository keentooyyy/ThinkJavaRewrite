using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using GameInteraction;
using GameEvents;
using System.Collections.Generic;

namespace NodeCanvas.Tasks.Actions
{
    [Category("✫ Custom/Interaction")]
    [Description("Evaluates filled puzzle slot pairs independently. Rejects incorrect pairs, triggers success when all filled pairs are correct.")]
    public class EvaluatePuzzleCombination : ActionTask
    {
        [Tooltip("Reference to the PickupPuzzleFSMController (auto-resolves from agent if not set)")]
        public BBParameter<PickupPuzzleFSMController> puzzleController;

        protected override string info => "Evaluate Filled Puzzle Pairs";

        protected override void OnExecute()
        {
            // Auto-resolve controller from agent (the FSM owner) if blackboard variable is not set
            var controller = puzzleController.value;
            if (controller == null && agent != null)
            {
                controller = agent as PickupPuzzleFSMController ?? agent.GetComponent<PickupPuzzleFSMController>();
            }
            
            if (controller == null)
            {
                EndAction(false);
                return;
            }

            var config = controller.PuzzleConfig;
            if (config == null)
            {
                EndAction(false);
                return;
            }

            var slotPairs = controller.SlotPairs;
            if (slotPairs == null || slotPairs.Length == 0)
            {
                EndAction(false);
                return;
            }

            // Only evaluate pairs that are filled - each pair works independently
            var filledPairs = new List<(PickupPuzzleFSMController.SlotPair pair, PickupPuzzleConfig.RequiredPair config, string successEvent)>();
            var slotsToReject = new List<(PickupSlot slot, PickupPuzzleFSMController.SlotPair pair)>();

            foreach (var slotPair in slotPairs)
            {
                var dataSlot = slotPair.dataTypeSlot;
                var varSlot = slotPair.variableSlot;

                if (dataSlot == null || varSlot == null)
                {
                    continue; // Skip invalid pairs
                }

                // Only evaluate pairs that are filled
                if (!dataSlot.HasItem || !varSlot.HasItem)
                {
                    continue; // Skip unfilled pairs - they work independently
                }

                // Try to get the required pair from config
                if (!config.TryGetPair(dataSlot.SlotDefinition, varSlot.SlotDefinition, out var requiredPair))
                {
                    // Slot combination not found in config - reject both slots
                    if (dataSlot.HasItem) slotsToReject.Add((dataSlot, slotPair));
                    if (varSlot.HasItem) slotsToReject.Add((varSlot, slotPair));
                    continue;
                }

                // Check if the placed items match the requirements
                var actualType = dataSlot.CurrentDataType;
                var actualVariable = varSlot.CurrentVariableNormalized;
                var expectedVariable = PickupMetadata.NormalizeIdentifier(requiredPair.requiredVariableName);

                bool typeMatches = actualType == requiredPair.requiredType;
                bool variableMatches = actualVariable == expectedVariable;

                // Determine which slots are wrong and reject only those
                if (!typeMatches && dataSlot.HasItem)
                {
                    slotsToReject.Add((dataSlot, slotPair));
                }

                if (!variableMatches && varSlot.HasItem)
                {
                    slotsToReject.Add((varSlot, slotPair));
                }

                // If both are correct, this pair is valid
                if (typeMatches && variableMatches)
                {
                    // Determine success event: use pair-specific event, then config event, then nothing
                    string successEvent = !string.IsNullOrEmpty(slotPair.successEventName) 
                        ? slotPair.successEventName 
                        : !string.IsNullOrEmpty(requiredPair.successEventName) 
                            ? requiredPair.successEventName 
                            : null;
                    
                    filledPairs.Add((slotPair, requiredPair, successEvent));
                }
            }

            // Reject incorrect slots (only the ones that are wrong, not both)
            if (slotsToReject.Count > 0)
            {
                foreach (var (slot, _) in slotsToReject)
                {
                    if (slot != null)
                    {
                        slot.RejectCurrent();
                    }
                }
                EndAction(false);
                return;
            }

            // If no pairs are filled, don't evaluate
            if (filledPairs.Count == 0)
            {
                EndAction(false);
                return;
            }

            // Trigger success events for correctly filled pairs that have events configured
            // User controls which pairs trigger which events - events fire when that pair is correct
            foreach (var (_, _, successEvent) in filledPairs)
            {
                if (!string.IsNullOrEmpty(successEvent))
                {
                    UIEventManager.Trigger(successEvent);
                }
            }

            // Check if all pairs are filled (required for full puzzle success)
            bool allPairsFilled = controller.AreAllSlotPairsFilled();

            // Success: all pairs are filled and correct (events already triggered above for configured pairs)
            if (allPairsFilled)
            {
                EndAction(true);
                return;
            }

            // Not all pairs are filled yet, but the filled ones are correct - keep waiting
            EndAction(false);
        }
    }
}

