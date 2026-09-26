using BrudvikStackedChest.Constants;
using UnityEngine;

namespace BrudvikStackedChest.Models
{
    /// <summary>
    /// Model class representing the configuration for a custom chest.
    /// </summary>
    public class CustomChestModel
    {
        /// <summary>
        /// The internal name of the chest (prefab name).
        /// </summary>
        public string Name { get; set; } = "BrudvikStackedChest";

        /// <summary>
        /// The display name shown in-game.
        /// </summary>
        public string DisplayName { get; set; } = "Brudvik Stacked Chest";

        /// <summary>
        /// The description shown in the build menu.
        /// </summary>
        public string Description { get; set; } = "BrudvikStackedChestDesc";

        /// <summary>
        /// The icon filename for the chest.
        /// </summary>
        public string Icon { get; set; } = "strg_049_round.png";

        /// <summary>
        /// The category of items the chest is filled with. The items are sorted automatically at runtime.
        /// </summary>
        public ChestCategory Category { get; set; } = ChestCategory.None;

        /// <summary>
        /// The color tint applied to the chest.
        /// </summary>
        public Color Color { get; set; } = new Color(0, 0, 0, 0.8f);

        /// <summary>
        /// The minimum number of rows in the chest inventory. Default is 8. The chest grows when it needs more room.
        /// </summary>
        public int Rows { get; set; } = 8;

        /// <summary>
        /// The number of columns in the chest inventory. Default is 8.
        /// </summary>
        public int Columns { get; set; } = 8;
    }
}
