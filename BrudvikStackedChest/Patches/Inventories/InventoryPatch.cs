using BrudvikStackedChest.Events;
using HarmonyLib;
using System;

namespace BrudvikStackedChest.Patches.Inventories
{
    /// <summary>
    /// Patches for the Inventory class that let handlers change how items are moved into and out of chests.
    /// </summary>
    public class InventoryPatch
    {
        /// <summary>
        /// Event triggered before an item is added by ctrl-click, "Place stacks" or another mod, or dropped on a slot.
        /// </summary>
        public static event EventHandler<InventoryAddingItemPatchEvent>? InventoryAddingItemPatched;

        /// <summary>
        /// Event triggered before every item of one inventory is moved into another.
        /// </summary>
        public static event EventHandler<InventoryMoveAllPatchEvent>? InventoryMoveAllPatched;

        private static bool ShouldAbsorb(Inventory inventory, ItemDrop.ItemData item)
        {
            if (inventory == null || item == null || InventoryAddingItemPatched == null) return false;

            var args = new InventoryAddingItemPatchEvent { Inventory = inventory, Item = item };
            InventoryAddingItemPatched.Invoke(null, args);
            return args.Absorb;
        }

        /// <summary>
        /// Harmony patch for Inventory.AddItem(ItemData), used by ctrl-click and the "Place stacks" button.
        /// The inventory's own loading uses another overload, so saved chests are never affected.
        /// </summary>
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), new[] { typeof(ItemDrop.ItemData) })]
        public static class InventoryAddItemPatch
        {
            static bool Prefix(Inventory __instance, ItemDrop.ItemData item, ref bool __result)
            {
                if (!ShouldAbsorb(__instance, item)) return true;

                // The caller removes the item from where it came from.
                __result = true;
                return false;
            }
        }

        /// <summary>
        /// Harmony patch for Inventory.MoveItemToThis with a target slot, used when an item is dropped on the grid.
        /// </summary>
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveItemToThis),
            new[] { typeof(Inventory), typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int) })]
        public static class InventoryMoveItemToThisPatch
        {
            static bool Prefix(Inventory __instance, Inventory fromInventory, ItemDrop.ItemData item, int amount, ref bool __result)
            {
                if (fromInventory == null || fromInventory == __instance || !ShouldAbsorb(__instance, item)) return true;

                fromInventory.RemoveItem(item, amount);
                __result = true;
                return false;
            }
        }

        /// <summary>
        /// Harmony patch for Inventory.MoveAll, used by the Take all button.
        /// </summary>
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.MoveAll))]
        public static class InventoryMoveAllPatch
        {
            static bool Prefix(Inventory __instance, Inventory fromInventory)
            {
                if (__instance == null || fromInventory == null || InventoryMoveAllPatched == null) return true;

                var args = new InventoryMoveAllPatchEvent { Inventory = __instance, FromInventory = fromInventory };
                InventoryMoveAllPatched.Invoke(null, args);
                return !args.Handled;
            }
        }
    }
}
