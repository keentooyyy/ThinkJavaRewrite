using UnityEngine;
using NodeCanvas.StateMachines;
using System.Collections.Generic;
using ParadoxNotion.Services;
using ParadoxNotion;

namespace GameInteraction
{
    /// <summary>
    /// FSM-based Pickup Puzzle Controller. Uses NodeCanvas FSM to handle puzzle logic.
    /// Supports multiple slot pairs per controller.
    /// </summary>
    [AddComponentMenu("Game/Pickup Puzzle FSM Controller")]
    public class PickupPuzzleFSMController : FSMOwner, IPickupPuzzleController
    {
        [System.Serializable]
        public struct SlotPair
        {
            [Tooltip("The data type slot for this pair")]
            public PickupSlot dataTypeSlot;
            
            [Tooltip("The variable slot for this pair")]
            public PickupSlot variableSlot;

            [Tooltip("Optional UI event name to trigger when this pair is correctly filled. Leave empty for no event. Overrides config event if set.")]
            public string successEventName;
        }

        [Header("Puzzle Configuration")]
        [SerializeField] private PickupPuzzleConfig puzzleConfig;

        [Header("Slot Pairs")]
        [Tooltip("List of slot pairs. Each pair works independently - they can be filled in any order and evaluate as they're filled. Puzzle succeeds when all pairs are filled and correct.")]
        [SerializeField] private SlotPair[] slotPairs = new SlotPair[1];

        public PickupPuzzleConfig PuzzleConfig => puzzleConfig;
        public SlotPair[] SlotPairs => slotPairs;
        public int SlotPairCount => slotPairs != null ? slotPairs.Length : 0;

        // Backwards compatibility - returns first pair
        public PickupSlot DataTypeSlot => slotPairs != null && slotPairs.Length > 0 ? slotPairs[0].dataTypeSlot : null;
        public PickupSlot VariableSlot => slotPairs != null && slotPairs.Length > 0 ? slotPairs[0].variableSlot : null;

        /// <summary>
        /// Gets all slot pairs that are currently filled
        /// </summary>
        public IEnumerable<SlotPair> GetFilledSlotPairs()
        {
            if (slotPairs == null) yield break;
            
            foreach (var pair in slotPairs)
            {
                if (pair.dataTypeSlot != null && pair.variableSlot != null &&
                    pair.dataTypeSlot.HasItem && pair.variableSlot.HasItem)
                {
                    yield return pair;
                }
            }
        }

        /// <summary>
        /// Checks if all slot pairs are filled
        /// </summary>
        public bool AreAllSlotPairsFilled()
        {
            if (slotPairs == null || slotPairs.Length == 0) return false;
            
            foreach (var pair in slotPairs)
            {
                if (pair.dataTypeSlot == null || pair.variableSlot == null)
                    return false;
                    
                if (!pair.dataTypeSlot.HasItem || !pair.variableSlot.HasItem)
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// Checks if any slot pair is filled (for independent pair evaluation)
        /// </summary>
        public bool HasAnySlotPairFilled()
        {
            if (slotPairs == null || slotPairs.Length == 0) return false;
            
            foreach (var pair in slotPairs)
            {
                if (pair.dataTypeSlot != null && pair.variableSlot != null &&
                    pair.dataTypeSlot.HasItem && pair.variableSlot.HasItem)
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Called by slots whenever their occupancy changes. Can be used to trigger FSM events.
        /// </summary>
        public void NotifySlotChanged(PickupSlot slot)
        {
            // Send event to FSM to trigger evaluation (bypass Graph.SendEvent to avoid logging)
            if (graph != null && graph.isRunning)
            {
                var router = gameObject.GetAddComponent<EventRouter>();
                router.InvokeCustomEvent("OnSlotChanged", null, this);
            }
        }
    }
}

