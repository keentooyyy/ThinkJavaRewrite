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
                Debug.LogWarning("PickupPuzzleController: Missing puzzle config.");
                return;
            }

            if (!puzzleConfig.TryGetPair(dataTypeSlot.SlotDefinition, variableSlot.SlotDefinition, out var pair))
            {
                Debug.LogWarning($"PickupPuzzleController: No required pair found for data slot '{dataTypeSlot.SlotId}' and variable slot '{variableSlot.SlotId}'.");
                return;
            }

            var actualType = dataTypeSlot.CurrentDataType;
            var actualVariable = variableSlot.CurrentVariableNormalized;
            var expectedVariable = PickupMetadata.NormalizeIdentifier(pair.requiredVariableName);

            bool typeMatches = actualType == pair.requiredType;
            bool variableMatches = actualVariable == expectedVariable;

            if (typeMatches && variableMatches)
            {
                Debug.Log($"Puzzle solved: {FormatCombo(actualType, variableSlot.CurrentVariableRaw)}");
            }
            else
            {
                Debug.LogWarning($"Puzzle failed: expected {FormatCombo(pair.requiredType, pair.requiredVariableName)}, got {FormatCombo(actualType, variableSlot.CurrentVariableRaw)}");
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


