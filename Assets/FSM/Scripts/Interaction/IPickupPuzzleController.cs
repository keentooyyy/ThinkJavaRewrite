namespace GameInteraction
{
    /// <summary>
    /// Interface for puzzle controllers to notify when slots change
    /// </summary>
    public interface IPickupPuzzleController
    {
        void NotifySlotChanged(PickupSlot slot);
    }
}

