using BrudvikStackedChest.Constants;
using BrudvikStackedChest.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BrudvikStackedChest.Extensions
{
    /// <summary>
    /// Extension methods for the Container class to handle item spawning and refilling.
    /// </summary>
    public static class ContainerExtension
    {
        // Remembers the mode a chest was last restocked in, so a mode change can be cleaned up.
        private const string ModeKey = "BrudvikStackedChest_Mode";

        /// <summary>
        /// Keeps the unlimited stacks in the container full and adds the unlimited items that are missing.
        /// Only the owner of the container changes it, and it is only saved when something actually changed.
        /// </summary>
        /// <param name="container">The container to restock.</param>
        /// <param name="category">The chest's item category.</param>
        /// <param name="supply">Decides which items are unlimited.</param>
        public static void Restock(this Container container, ChestCategory category, ChestSupply supply)
        {
            if (!IsOwnedByMe(container) || !supply.IsReady) return;

            var inventory = container.GetInventory();
            if (inventory == null) return;

            var mode = supply.Mode;
            var changed = RemoveAfterModeChange(container, inventory, category, supply, mode);
            if (mode == ChestMode.Linear) UnlockFullStacks(container, inventory, category, supply);

            var refilled = false;
            var present = new HashSet<string>();
            foreach (var item in inventory.GetAllItems())
            {
                if (item.m_dropPrefab != null) present.Add(item.m_dropPrefab.name);

                if (item.m_stack < item.m_shared.m_maxStackSize && supply.IsSupplied(mode, category, item))
                {
                    item.m_stack = item.m_shared.m_maxStackSize;
                    refilled = true;
                }
            }

            changed |= refilled;
            if (refilled) ChestEffects.PlayRefill(container);

            var missing = new List<GameObject>();
            foreach (var prefabName in supply.GetItemsToAdd(category))
            {
                if (present.Contains(prefabName)) continue;

                var prefab = ObjectDB.instance.GetItemPrefab(prefabName);
                if (prefab != null && prefab.GetComponent<ItemDrop>() != null) missing.Add(prefab);
            }

            if (missing.Count > 0)
            {
                EnsureRows(inventory, inventory.GetAllItems().Count + missing.Count);
                foreach (var prefab in missing)
                {
                    var maxStack = prefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_maxStackSize;
                    changed |= inventory.AddItem(prefab, maxStack);
                }
            }

            if (changed) container.Save();
        }

        /// <summary>
        /// Removes the stacks the chest supplies without limit, so only items players stored themselves are dropped
        /// when the chest is destroyed.
        /// </summary>
        /// <param name="container">The container that is about to drop its items.</param>
        /// <param name="category">The chest's item category.</param>
        /// <param name="supply">Decides which items are unlimited.</param>
        public static void RemoveSuppliedItems(this Container container, ChestCategory category, ChestSupply supply)
        {
            var inventory = container.GetInventory();
            if (inventory == null) return;

            var mode = supply.Mode;
            if (RemoveWhere(inventory, item => supply.IsSupplied(mode, category, item))) container.Save();
        }

        private static bool RemoveAfterModeChange(Container container, Inventory inventory, ChestCategory category, ChestSupply supply, ChestMode mode)
        {
            var zdo = container.m_nview.GetZDO();

            // Chests from before the modes existed were filled in Full mode.
            var previous = (ChestMode)zdo.GetInt(ModeKey, (int)ChestMode.Full);
            if (previous == mode) return false;

            zdo.Set(ModeKey, (int)mode);
            return RemoveWhere(inventory, item => supply.IsSupplied(previous, category, item) && !supply.IsSupplied(mode, category, item));
        }

        private static void UnlockFullStacks(Container container, Inventory inventory, ChestCategory category, ChestSupply supply)
        {
            var totals = new Dictionary<string, int>();
            var shared = new Dictionary<string, ItemDrop.ItemData.SharedData>();
            foreach (var item in inventory.GetAllItems())
            {
                if (item.m_dropPrefab == null) continue;

                var name = item.m_dropPrefab.name;
                totals.TryGetValue(name, out var total);
                totals[name] = total + item.m_stack;
                shared[name] = item.m_shared;
            }

            foreach (var pair in totals)
            {
                var itemShared = shared[pair.Key];
                if (!supply.CanUnlock(category, pair.Key, itemShared) || pair.Value < supply.GetUnlockAmount(itemShared)) continue;

                supply.Unlock(pair.Key);
                ChestEffects.Play(container, ChestEffects.Unlock);
                if (Player.m_localPlayer != null)
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center,
                        $"{Localization.instance.Localize(itemShared.m_name)} is now unlimited");
                }
            }
        }

        private static bool RemoveWhere(Inventory inventory, Func<ItemDrop.ItemData, bool> predicate)
        {
            var removed = false;
            foreach (var item in inventory.GetAllItems().ToList())
            {
                if (predicate(item)) removed |= inventory.RemoveItem(item);
            }
            return removed;
        }

        /// <summary>
        /// Grows the inventory height so it can hold the given number of stacks. Other clients follow automatically,
        /// because the game expands a container to fit the saved item positions when it loads.
        /// </summary>
        private static void EnsureRows(Inventory inventory, int stacks)
        {
            var width = Mathf.Max(1, inventory.GetWidth());
            var rows = (stacks + width - 1) / width;
            if (rows > inventory.GetHeight()) inventory.SetHeight(rows);
        }

        /// <summary>
        /// Checks that the container is a placed, networked instance owned by this peer. Only the owner may change
        /// it; changes made elsewhere would be overwritten or fight over the shared data.
        /// </summary>
        private static bool IsOwnedByMe(Container container)
        {
            return container.m_nview != null && container.m_nview.IsValid() && container.m_nview.IsOwner();
        }
    }
}
