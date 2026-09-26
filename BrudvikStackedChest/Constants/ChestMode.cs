namespace BrudvikStackedChest.Constants
{
    /// <summary>
    /// Decides which items a chest supplies without limit.
    /// </summary>
    public enum ChestMode
    {
        /// <summary>Every item of the chest's category is always available.</summary>
        Full,

        /// <summary>An item becomes unlimited, world-wide, once a full stack of it has been stored in a chest.</summary>
        Linear,

        /// <summary>An item becomes unlimited, world-wide, once any player in the world has discovered it.</summary>
        Discovered
    }
}
