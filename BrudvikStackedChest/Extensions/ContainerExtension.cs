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

        // Restocking changes the inventory, which reports the change and would restock again.
        private static bool restocking;

        /// <summary>
        /// Keeps the unlimited stacks in the container full, removes surplus stacks, combines partial stacks and adds
        /// the unlimited items that are missing. Only the owner of the container changes it, and it is only saved
        /// when something actually changed.
        /// </summary>
        /// <param name="container">The container to restock.</param>
        /// <param name="category">The chest's item category.</param>
        /// <param name="supply">Decides which items are unlimited.</param>
        /// <param name="sort">Whether to keep the contents sorted from the top left.</param>
        public static void Restock(this Container container, ChestCategory category, ChestSupply supply, bool sort)
        {
            if (restocking || !IsOwnedByMe(container) || !supply.IsReady) return;

            var inventory = container.GetInventory();
            if (inventory == null) return;

            restocking = true;
            try
            {
                var mode = supply.Mode;
                var changed = RemoveAfterModeChange(container, inventory, category, supply, mode);
                if (mode == ChestMode.Linear) UnlockFullStacks(container, inventory, category, supply);

                changed |= MergeStacks(inventory, category, supply, mode, out var refilled);
                if (refilled) ChestEffects.PlayRefill(container);

                changed |= AddMissing(inventory, category, supply);
                if (sort) changed |= Arrange(inventory, category, supply, mode);

                if (changed) container.Save();
            }
            finally
            {
                restocking = false;
            }
        }

        /// <summary>
        /// Removes the stacks that belong to the chest itself, so only items players stored themselves are dropped
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
            if (RemoveWhere(inventory, item => supply.IsStock(mode, category, item))) container.Save();
        }

        /// <summary>
        /// Moves the items players stored themselves to another inventory and leaves the chest's own stacks, so Take
        /// all does not fill the player's inventory with unlimited stacks.
        /// </summary>
        /// <param name="container">The chest to take from.</param>
        /// <param name="target">The inventory that receives the items.</param>
        /// <param name="category">The chest's item category.</param>
        /// <param name="supply">Decides which items are unlimited.</param>
        public static void MoveStoredItemsTo(this Container container, Inventory target, ChestCategory category, ChestSupply supply)
        {
            var inventory = container.GetInventory();
            if (inventory == null) return;

            var mode = supply.Mode;
            foreach (var item in inventory.GetAllItems().ToList())
            {
                if (!supply.IsStock(mode, category, item)) target.MoveItemToThis(inventory, item);
            }
        }

        private static bool RemoveAfterModeChange(Container container, Inventory inventory, ChestCategory category, ChestSupply supply, ChestMode mode)
        {
            var zdo = container.m_nview.GetZDO();

            // Chests from before the modes existed were filled in Full mode.
            var previous = (ChestMode)zdo.GetInt(ModeKey, (int)ChestMode.Full);
            if (previous == mode) return false;

            zdo.Set(ModeKey, (int)mode);
            return RemoveWhere(inventory, item => supply.IsStock(previous, category, item) && !supply.IsSupplied(mode, category, item));
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

        /// <summary>
        /// Keeps the wanted number of full stacks of every unlimited item and removes the rest, and combines partial
        /// stacks of the other items.
        /// </summary>
        private static bool MergeStacks(Inventory inventory, ChestCategory category, ChestSupply supply, ChestMode mode, out bool refilled)
        {
            refilled = false;
            var changed = false;
            var kept = new Dictionary<string, int>();
            var partial = new Dictionary<string, ItemDrop.ItemData>();

            foreach (var item in InGridOrder(inventory))
            {
                var maxStack = item.m_shared.m_maxStackSize;
                if (maxStack <= 1) continue;

                if (item.m_dropPrefab != null && supply.IsSupplied(mode, category, item))
                {
                    var name = item.m_dropPrefab.name;
                    kept.TryGetValue(name, out var count);
                    if (count >= supply.GetStackCount(name, item.m_shared))
                    {
                        changed |= inventory.RemoveItem(item);
                        continue;
                    }

                    kept[name] = count + 1;
                    if (item.m_stack < maxStack)
                    {
                        item.m_stack = maxStack;
                        refilled = true;
                    }
                    continue;
                }

                // The game combines stacks with the same name, quality and world level.
                var key = $"{item.m_shared.m_name}|{item.m_quality}|{item.m_worldLevel}";
                if (partial.TryGetValue(key, out var target))
                {
                    var moved = Math.Min(maxStack - target.m_stack, item.m_stack);
                    target.m_stack += moved;
                    item.m_stack -= moved;
                    changed = true;

                    if (target.m_stack >= maxStack) partial.Remove(key);
                    if (item.m_stack <= 0)
                    {
                        inventory.RemoveItem(item);
                        continue;
                    }
                }

                if (item.m_stack < maxStack) partial[key] = item;
            }

            return changed || refilled;
        }

        private static bool AddMissing(Inventory inventory, ChestCategory category, ChestSupply supply)
        {
            var present = new Dictionary<string, int>();
            foreach (var item in inventory.GetAllItems())
            {
                if (item.m_dropPrefab == null) continue;

                present.TryGetValue(item.m_dropPrefab.name, out var count);
                present[item.m_dropPrefab.name] = count + 1;
            }

            var missing = new List<ItemDrop.ItemData>();
            foreach (var prefabName in supply.GetItemsToAdd(category))
            {
                var prefab = ObjectDB.instance.GetItemPrefab(prefabName);
                var itemDrop = prefab == null ? null : prefab.GetComponent<ItemDrop>();
                if (itemDrop == null) continue;

                present.TryGetValue(prefabName, out var count);
                var wanted = supply.GetStackCount(prefabName, itemDrop.m_itemData.m_shared);
                for (var i = count; i < wanted; i++)
                {
                    missing.Add(CreateFullStack(prefab!, itemDrop));
                }
            }

            if (missing.Count == 0) return false;

            EnsureRows(inventory, inventory.GetAllItems().Count + missing.Count);
            var added = false;
            foreach (var item in missing)
            {
                // The game fills materials from the bottom row; chests fill from the top.
                var slot = inventory.FindEmptySlot(true);
                if (slot.x < 0) break;

                added |= inventory.AddItem(item, slot);
            }
            return added;
        }

        private static ItemDrop.ItemData CreateFullStack(GameObject prefab, ItemDrop itemDrop)
        {
            var item = itemDrop.m_itemData.Clone();
            item.m_dropPrefab = prefab;
            item.m_stack = item.m_shared.m_maxStackSize;
            item.m_worldLevel = (byte)Game.m_worldLevel;
            return item;
        }

        /// <summary>
        /// Places the contents row by row from the top left: unlimited items first, then items stored by players,
        /// each by item type and name.
        /// </summary>
        private static bool Arrange(Inventory inventory, ChestCategory category, ChestSupply supply, ChestMode mode)
        {
            var ordered = inventory.GetAllItems()
                .OrderBy(item => supply.IsSupplied(mode, category, item) ? 0 : 1)
                .ThenBy(item => item.m_shared.m_itemType)
                .ThenBy(item => Localization.instance.Localize(item.m_shared.m_name), StringComparer.CurrentCultureIgnoreCase)
                .ThenByDescending(item => item.m_quality)
                .ThenByDescending(item => item.m_stack)
                .ThenBy(item => item.m_gridPos.y)
                .ThenBy(item => item.m_gridPos.x)
                .ToList();

            var width = Mathf.Max(1, inventory.GetWidth());
            var moved = false;
            for (var i = 0; i < ordered.Count; i++)
            {
                var x = i % width;
                var y = i / width;
                var item = ordered[i];
                if (item.m_gridPos.x == x && item.m_gridPos.y == y) continue;

                item.m_gridPos = new Vector2i(x, y);
                moved = true;
            }
            return moved;
        }

        private static List<ItemDrop.ItemData> InGridOrder(Inventory inventory)
        {
            return inventory.GetAllItems().OrderBy(item => item.m_gridPos.y).ThenBy(item => item.m_gridPos.x).ToList();
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
