namespace GameInteraction
{
    /// <summary>
    /// Provides the name of the action button required to interact with an object.
    /// </summary>
    public interface IActionButtonProvider
    {
        string RequiredButton { get; }
    }
}

