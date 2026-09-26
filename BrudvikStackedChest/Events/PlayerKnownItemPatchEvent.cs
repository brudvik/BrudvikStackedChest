using System;

namespace BrudvikStackedChest.Events
{
    /// <summary>
    /// Raised when the local player learns about an item.
    /// </summary>
    public class PlayerKnownItemPatchEvent : EventArgs
    {
        /// <summary>
        /// The item's localization token (<c>m_shared.m_name</c>), which is how the game records known items.
        /// </summary>
        public string ItemToken { get; set; } = string.Empty;
    }
}
