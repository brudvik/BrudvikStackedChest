using System;

namespace BrudvikStackedChest.Events
{
    /// <summary>
    /// Raised after a container has built its hover text. Handlers may change <see cref="Text"/>.
    /// </summary>
    public class ContainerHoverTextPatchEvent : EventArgs
    {
        /// <summary>
        /// The hovered container.
        /// </summary>
        public Container Container { get; set; }

        /// <summary>
        /// The hover text shown to the player.
        /// </summary>
        public string Text { get; set; } = string.Empty;
    }
}
