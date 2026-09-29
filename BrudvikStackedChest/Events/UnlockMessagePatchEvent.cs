using System;

namespace BrudvikStackedChest.Events
{
    /// <summary>
    /// Raised before the game queues a "new item", "new recipe" or similar message.
    /// </summary>
    public class UnlockMessagePatchEvent : EventArgs
    {
        /// <summary>
        /// The message topic, such as <c>$msg_newrecipe</c>.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Set to true to leave the message out.
        /// </summary>
        public bool Skip { get; set; }
    }
}
