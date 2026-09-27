using System;

namespace BrudvikStackedChest.Events
{
    /// <summary>
    /// Raised before every item of one inventory is moved into another, as the Take all button does. Handlers may
    /// do the move themselves and set <see cref="Handled"/> to skip the game's own move.
    /// </summary>
    public class InventoryMoveAllPatchEvent : EventArgs
    {
        /// <summary>
        /// The inventory that receives the items.
        /// </summary>
        public Inventory Inventory { get; set; }

        /// <summary>
        /// The inventory the items are taken from.
        /// </summary>
        public Inventory FromInventory { get; set; }

        /// <summary>
        /// When true, the game's own move is skipped.
        /// </summary>
        public bool Handled { get; set; }
    }
}
