using System;

namespace BrudvikStackedChest.Events
{
    /// <summary>
    /// Raised after an inventory grid has updated its slots.
    /// </summary>
    public class InventoryGridUpdatedPatchEvent : EventArgs
    {
        /// <summary>
        /// The updated grid.
        /// </summary>
        public InventoryGrid Grid { get; set; }
    }
}
