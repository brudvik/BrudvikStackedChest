using System.Collections.Generic;
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
        /// List of item prefab names to spawn in the chest.
        /// </summary>
        public List<string> SpawnItems { get; set; } = new List<string>();

        /// <summary>
        /// The color tint applied to the chest.
        /// </summary>
        public Color Color { get; set; } = new Color(0, 0, 0, 0.8f);

        /// <summary>
        /// The number of rows in the chest inventory. Default is 8.
        /// </summary>
        public int Rows { get; set; } = 8;

        /// <summary>
        /// The number of columns in the chest inventory. Default is 8.
        /// </summary>
        public int Columns { get; set; } = 8;
    }
}
