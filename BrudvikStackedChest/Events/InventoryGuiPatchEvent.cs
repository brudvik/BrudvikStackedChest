using System;

namespace BrudvikStackedChest.Events
{
    /// <summary>
    /// Raised when the inventory screen is shown, hidden or opens one of its own panels.
    /// </summary>
    public class InventoryGuiPatchEvent : EventArgs
    {
        /// <summary>
        /// The inventory screen.
        /// </summary>
        public InventoryGui Gui { get; set; }
    }
}
