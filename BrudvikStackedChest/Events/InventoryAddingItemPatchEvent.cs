using System;

namespace BrudvikStackedChest.Events
{
    /// <summary>
    /// Raised before an item is moved or added into an inventory. Handlers may set <see cref="Absorb"/> to take the
    /// item away from its source without adding it.
    /// </summary>
    public class InventoryAddingItemPatchEvent : EventArgs
    {
        /// <summary>
        /// The inventory the item is added to.
        /// </summary>
        public Inventory Inventory { get; set; }

        /// <summary>
        /// The item being added.
        /// </summary>
        public ItemDrop.ItemData Item { get; set; }

        /// <summary>
        /// When true, the item is removed from its source but not added to the inventory.
        /// </summary>
        public bool Absorb { get; set; }
    }
}
