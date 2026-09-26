using System;

namespace BrudvikStackedChest.Events
{
    /// <summary>
    /// Raised when the local player has spawned in the world.
    /// </summary>
    public class PlayerSpawnedPatchEvent : EventArgs
    {
        /// <summary>
        /// The local player.
        /// </summary>
        public Player Player { get; set; }
    }
}
