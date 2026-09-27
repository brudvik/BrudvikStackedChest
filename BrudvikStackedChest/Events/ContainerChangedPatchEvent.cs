using System;

namespace BrudvikStackedChest.Events
{
    /// <summary>
    /// Raised after the contents of a container have changed.
    /// </summary>
    public class ContainerChangedPatchEvent : EventArgs
    {
        /// <summary>
        /// The changed container.
        /// </summary>
        public Container Container { get; set; }
    }
}
