using UnityEngine;

namespace GameInteraction
{
    [CreateAssetMenu(menuName = "Puzzle/Pickup Slot Definition", fileName = "PickupSlotDefinition")]
    public class PickupSlotDefinition : ScriptableObject
    {
        [SerializeField]
        private string slotId = "slot";

        public string SlotId => slotId;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(slotId))
            {
                slotId = name;
            }
        }
#endif
    }
}


