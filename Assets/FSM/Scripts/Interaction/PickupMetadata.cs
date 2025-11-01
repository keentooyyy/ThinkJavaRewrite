namespace GameInteraction
{
    /// <summary>
    /// Represents the basic data types that a pickup can be associated with.
    /// Mirrors primitive script types for matching with slots.
    /// </summary>
    public enum ScriptDataType
    {
        None,
        String,
        Int,
        Float,
        Bool,
        Custom
    }

    public static class PickupMetadata
    {
        /// <summary>
        /// Normalizes an identifier for case-insensitive comparisons and trims whitespace.
        /// Returns an empty string if the value is null or whitespace.
        /// </summary>
        public static string NormalizeIdentifier(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        }
    }
}

