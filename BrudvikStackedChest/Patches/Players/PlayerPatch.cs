using BrudvikStackedChest.Events;
using HarmonyLib;
using System;

namespace BrudvikStackedChest.Patches.Players
{
    /// <summary>
    /// Patches for the Player class that raise events for the local player.
    /// </summary>
    public class PlayerPatch
    {
        /// <summary>
        /// Event triggered after the local player has spawned.
        /// </summary>
        public static event EventHandler<PlayerSpawnedPatchEvent>? PlayerSpawnedPatched;

        /// <summary>
        /// Event triggered after the local player has learned about an item.
        /// </summary>
        public static event EventHandler<PlayerKnownItemPatchEvent>? PlayerKnownItemPatched;

        /// <summary>
        /// Harmony patch for Player.OnSpawned.
        /// </summary>
        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static class PlayerOnSpawnedPatch
        {
            static void Postfix(Player __instance)
            {
                if (__instance != null && __instance == Player.m_localPlayer)
                {
                    PlayerSpawnedPatched?.Invoke(null, new PlayerSpawnedPatchEvent { Player = __instance });
                }
            }
        }

        /// <summary>
        /// Harmony patch for Player.AddKnownItem.
        /// </summary>
        [HarmonyPatch(typeof(Player), "AddKnownItem")]
        public static class PlayerAddKnownItemPatch
        {
            static void Postfix(Player __instance, ItemDrop.ItemData item)
            {
                if (__instance != null && __instance == Player.m_localPlayer && item?.m_shared?.m_name != null)
                {
                    PlayerKnownItemPatched?.Invoke(null, new PlayerKnownItemPatchEvent { ItemToken = item.m_shared.m_name });
                }
            }
        }
    }
}
