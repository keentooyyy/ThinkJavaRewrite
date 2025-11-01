using UnityEngine;

namespace GameInteraction
{
    public enum PickupPuzzleKind
    {
        DataType,
        Variable
    }

    /// <summary>
    /// Metadata used by the pickup puzzle system to describe datatype and variable information.
    /// Attach alongside <see cref="Interactable"/> only on pickups that participate in the puzzle.
    /// </summary>
    [DisallowMultipleComponent]
    public class PickupPuzzleMetadata : MonoBehaviour
    {
        [Header("Pickup Puzzle Metadata")]
        [Tooltip("Determines whether this pickup represents a data type or a variable.")]
        public PickupPuzzleKind kind = PickupPuzzleKind.DataType;

        [Tooltip("The data type this pickup represents (used when Kind = DataType).")]
        public ScriptDataType dataType = ScriptDataType.String;

        [Tooltip("The variable name this pickup corresponds to (used when Kind = Variable).")]
        public string variableName = string.Empty;

        public bool HasDataType => kind == PickupPuzzleKind.DataType && dataType != ScriptDataType.None;
        public bool HasVariable => kind == PickupPuzzleKind.Variable && !string.IsNullOrWhiteSpace(variableName);

        public ScriptDataType EffectiveDataType => HasDataType ? dataType : ScriptDataType.None;
        public string EffectiveVariable => HasVariable ? variableName : string.Empty;
        public string EffectiveVariableNormalized => HasVariable ? PickupMetadata.NormalizeIdentifier(variableName) : string.Empty;
    }
}


