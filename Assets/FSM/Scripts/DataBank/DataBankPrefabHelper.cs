using UnityEngine;
using TMPro;

namespace GameDataBank
{
    /// <summary>
    /// Static helper to set text on data bank prefab instances.
    /// </summary>
    public static class DataBankPrefabHelper
    {
        /// <summary>
        /// Finds TextMeshProUGUI in children and sets its text.
        /// </summary>
        public static void SetPrefabText(GameObject prefabInstance, string text)
        {
            if (prefabInstance == null || string.IsNullOrEmpty(text))
            {
                return;
            }

            var textComponent = prefabInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent == null)
            {
                Debug.LogWarning($"SetPrefabText: '{prefabInstance.name}' has no TextMeshProUGUI child.");
                return;
            }

            textComponent.text = text;
        }
    }
}

