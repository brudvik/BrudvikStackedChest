namespace BrudvikStackedChest.Constants
{
    /// <summary>
    /// The item category a chest is filled with. Items are sorted into these automatically at runtime.
    /// </summary>
    public enum ChestCategory
    {
        /// <summary>No automatic contents; the chest only restocks what players put into it.</summary>
        None,
        Wood,
        Stone,
        Metal,
        Food,
        Material,
        Animal,
        Seed,
        Trophy,
        Treasure,
        Tools,
        Armor,
        Weapon,
        Potion
    }
}
