using System;
using UnityEngine;

namespace GameInteraction
{
    [CreateAssetMenu(menuName = "Puzzle/Pickup Puzzle Config", fileName = "PickupPuzzleConfig")]
    public class PickupPuzzleConfig : ScriptableObject
    {
        [Serializable]
        public struct RequiredPair
        {
            [Tooltip("Reference to the data-type slot definition for this pair.")]
            public PickupSlotDefinition dataSlot;

            [Tooltip("Reference to the variable slot definition for this pair.")]
            public PickupSlotDefinition variableSlot;

            [Tooltip("The required data type for the data slot.")]
            public ScriptDataType requiredType;

            [Tooltip("The required variable name for the variable slot.")]
            public string requiredVariableName;

            [Tooltip("Optional UI event name to trigger when this pair is correctly filled. Leave empty for no event.")]
            public string successEventName;
        }

        [Tooltip("List of valid data/variable slot combinations for this puzzle.")]
        public RequiredPair[] requiredPairs = Array.Empty<RequiredPair>();

        public bool TryGetPair(PickupSlotDefinition dataSlot, PickupSlotDefinition variableSlot, out RequiredPair pair)
        {
            foreach (var entry in requiredPairs)
            {
                if (entry.dataSlot == dataSlot && entry.variableSlot == variableSlot)
                {
                    pair = entry;
                    return true;
                }
            }

            pair = default;
            return false;
        }
    }
}


