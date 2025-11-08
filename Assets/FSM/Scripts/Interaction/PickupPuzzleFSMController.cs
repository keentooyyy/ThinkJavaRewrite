using UnityEngine;
using NodeCanvas.StateMachines;

namespace GameInteraction
{
    /// <summary>
    /// FSM-based Pickup Puzzle Controller. Uses NodeCanvas FSM to handle puzzle logic.
    /// </summary>
    [AddComponentMenu("Game/Pickup Puzzle FSM Controller")]
    public class PickupPuzzleFSMController : FSMOwner, IPickupPuzzleController
    {
        [Header("Slot References")]
        [SerializeField] private PickupSlot dataTypeSlot;
        [SerializeField] private PickupSlot variableSlot;

        [Header("Puzzle Configuration")]
        [SerializeField] private PickupPuzzleConfig puzzleConfig;

        [Header("UI Events")]
        [Tooltip("Event name to show the success UI when the correct pair is placed.")]
        [SerializeField] private string puzzleSuccessEvent = "ShowSuccessUI";

        public PickupSlot DataTypeSlot => dataTypeSlot;
        public PickupSlot VariableSlot => variableSlot;
        public PickupPuzzleConfig PuzzleConfig => puzzleConfig;
        public string PuzzleSuccessEvent => puzzleSuccessEvent;

        /// <summary>
        /// Called by slots whenever their occupancy changes. Can be used to trigger FSM events.
        /// </summary>
        public void NotifySlotChanged(PickupSlot slot)
        {
            // Send event to FSM to trigger evaluation
            SendEvent("OnSlotChanged");
        }
    }
}

