using BrudvikStackedChest.Events;
using HarmonyLib;
using System;

namespace BrudvikStackedChest.Patches.Gui
{
    /// <summary>
    /// Patches for the message display that let handlers leave out unlock messages.
    /// </summary>
    public class MessageHudPatch
    {
        /// <summary>
        /// Event triggered before an unlock message is queued.
        /// </summary>
        public static event EventHandler<UnlockMessagePatchEvent>? UnlockMessagePatched;

        /// <summary>
        /// Harmony patch for MessageHud.QueueUnlockMsg.
        /// </summary>
        [HarmonyPatch(typeof(MessageHud), "QueueUnlockMsg")]
        public static class MessageHudQueueUnlockMsgPatch
        {
            static bool Prefix(string topic)
            {
                if (UnlockMessagePatched == null) return true;

                var args = new UnlockMessagePatchEvent { Topic = topic };
                UnlockMessagePatched.Invoke(null, args);
                return !args.Skip;
            }
        }
    }
}
