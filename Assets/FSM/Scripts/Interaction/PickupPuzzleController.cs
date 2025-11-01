using UnityEngine;

namespace GameInteraction
{
    /// <summary>
    /// Coordinates datatype/variable slots and validates when both are filled.
    /// </summary>
    public class PickupPuzzleController : MonoBehaviour
    {
        [Header("Slot References")]
        [SerializeField] private PickupSlot dataTypeSlot;
        [SerializeField] private PickupSlot variableSlot;

        [Header("Puzzle Configuration")]
        [SerializeField] private PickupPuzzleConfig puzzleConfig;

        /// <summary>
        /// Called by slots whenever their occupancy changes. Triggers evaluation when both are filled.
        /// </summary>
        public void NotifySlotChanged(PickupSlot slot)
        {
            if (dataTypeSlot == null || variableSlot == null)
            {
                return;
            }

            if (!dataTypeSlot.HasItem || !variableSlot.HasItem)
            {
                return;
            }

            EvaluateCombination();
        }

        private void EvaluateCombination()
        {
            if (puzzleConfig == null)
            {
                return;
            }

            if (!puzzleConfig.TryGetPair(dataTypeSlot.SlotDefinition, variableSlot.SlotDefinition, out var pair))
            {
                return;
            }

            var actualType = dataTypeSlot.CurrentDataType;
            var actualVariable = variableSlot.CurrentVariableNormalized;
            var expectedVariable = PickupMetadata.NormalizeIdentifier(pair.requiredVariableName);

            bool typeMatches = actualType == pair.requiredType;
            bool variableMatches = actualVariable == expectedVariable;

            if (!typeMatches || !variableMatches)
            {
                dataTypeSlot.RejectCurrent();
                variableSlot.RejectCurrent();
            }
        }

        private static string FormatCombo(ScriptDataType type, string variableName)
        {
            var normalizedVariable = PickupMetadata.NormalizeIdentifier(variableName);
            var typeString = type.ToString().ToLowerInvariant();
            return string.IsNullOrEmpty(normalizedVariable) ? typeString : $"{typeString} {normalizedVariable}";
        }
    }
}


