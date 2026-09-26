using System;

namespace BrudvikStackedChest.Events
{
    /// <summary>
    /// Raised after the inventory screen has updated the open container panel.
    /// </summary>
    public class ContainerPanelUpdatedPatchEvent : EventArgs
    {
        /// <summary>
        /// The inventory screen.
        /// </summary>
        public InventoryGui Gui { get; set; }
    }
}
