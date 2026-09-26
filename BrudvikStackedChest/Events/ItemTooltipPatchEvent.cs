using System;

namespace BrudvikStackedChest.Events
{
    /// <summary>
    /// Raised after an inventory grid has built the tooltip of an item.
    /// </summary>
    public class ItemTooltipPatchEvent : EventArgs
    {
        /// <summary>
        /// The grid that shows the item.
        /// </summary>
        public InventoryGrid Grid { get; set; }

        /// <summary>
        /// The item the tooltip describes.
        /// </summary>
        public ItemDrop.ItemData Item { get; set; }

        /// <summary>
        /// The tooltip, which handlers may change.
        /// </summary>
        public UITooltip Tooltip { get; set; }
    }
}
