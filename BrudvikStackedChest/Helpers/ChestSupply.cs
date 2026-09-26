using BrudvikStackedChest.Configuration;
using BrudvikStackedChest.Constants;
using BrudvikStackedChest.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrudvikStackedChest.Helpers
{
    /// <summary>
    /// How far an item in a chest is from being unlocked.
    /// </summary>
    public class UnlockProgress
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnlockProgress"/> class.
        /// </summary>
        /// <param name="displayName">The localized item name.</param>
        /// <param name="stored">The amount in the chest.</param>
        /// <param name="required">The amount needed to unlock the item.</param>
        public UnlockProgress(string displayName, int stored, int required)
        {
            DisplayName = displayName;
            Stored = stored;
            Required = required;
        }

        /// <summary>The localized item name.</summary>
        public string DisplayName { get; }

        /// <summary>The amount in the chest.</summary>
        public int Stored { get; }

        /// <summary>The amount needed to unlock the item.</summary>
        public int Required { get; }
    }

    /// <summary>
    /// Decides, for the active <see cref="ChestMode"/>, which items a chest supplies without limit.
    /// </summary>
    public class ChestSupply
    {
        private readonly PluginSettings settings;
        private readonly ItemCatalog catalog;
        private readonly WorldProgress progress;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChestSupply"/> class.
        /// </summary>
        /// <param name="settings">The plugin settings.</param>
        /// <param name="catalog">The generated item lists per chest category.</param>
        /// <param name="progress">The world-wide unlocked and discovered items.</param>
        public ChestSupply(PluginSettings settings, ItemCatalog catalog, WorldProgress progress)
        {
            this.settings = settings;
            this.catalog = catalog;
            this.progress = progress;
        }

        /// <summary>
        /// Gets the active chest mode.
        /// </summary>
        public ChestMode Mode => settings.Mode.Value;

        /// <summary>
        /// Gets a value indicating whether the world-wide progress is known, which the progression modes depend on.
        /// </summary>
        public bool IsReady => Mode == ChestMode.Full || progress.IsReady;

        /// <summary>
        /// Checks whether a stack in a chest is refilled without limit under the given mode.
        /// </summary>
        /// <param name="mode">The chest mode to evaluate.</param>
        /// <param name="category">The chest's category; <see cref="ChestCategory.None"/> accepts any item.</param>
        /// <param name="item">The stack in the chest.</param>
        /// <returns>True if the stack is kept full.</returns>
        public bool IsSupplied(ChestMode mode, ChestCategory category, ItemDrop.ItemData item)
        {
            if (mode == ChestMode.Full) return true;
            if (item.m_dropPrefab == null) return false;

            return IsSupplied(mode, category, item.m_dropPrefab.name, item.m_shared);
        }

        /// <summary>
        /// Gets the items a chest of the given category is filled with when they are missing.
        /// </summary>
        /// <param name="category">The chest category.</param>
        /// <returns>The item prefab names.</returns>
        public IEnumerable<string> GetItemsToAdd(ChestCategory category)
        {
            var mode = Mode;
            foreach (var name in catalog.GetItems(category))
            {
                if (mode == ChestMode.Full)
                {
                    yield return name;
                    continue;
                }

                var shared = catalog.GetShared(name);
                if (shared != null && IsSupplied(mode, category, name, shared)) yield return name;
            }
        }

        /// <summary>
        /// Counts how many of a category's items are unlimited, and how many could become unlimited.
        /// </summary>
        /// <param name="category">The chest category.</param>
        /// <param name="supplied">The number of unlimited items.</param>
        /// <param name="total">The number of items that can become unlimited.</param>
        public void CountProgress(ChestCategory category, out int supplied, out int total)
        {
            var mode = Mode;
            supplied = 0;
            total = 0;
            foreach (var name in catalog.GetItems(category))
            {
                var shared = catalog.GetShared(name);
                if (shared == null || (mode != ChestMode.Full && !IsStackable(shared))) continue;

                total++;
                if (IsSupplied(mode, category, name, shared)) supplied++;
            }
        }

        /// <summary>
        /// Describes what happens to a stack in a chest, for the item tooltip.
        /// </summary>
        /// <param name="category">The chest's category; <see cref="ChestCategory.None"/> accepts any item.</param>
        /// <param name="item">The stack in the chest.</param>
        /// <param name="stored">The total amount of this item in the chest.</param>
        /// <returns>The status text.</returns>
        public string GetStatus(ChestCategory category, ItemDrop.ItemData item, int stored)
        {
            var mode = Mode;
            if (IsSupplied(mode, category, item)) return "Unlimited";
            if (item.m_dropPrefab == null) return "Stored normally";

            var name = item.m_dropPrefab.name;
            if (!BelongsIn(category, name)) return "Stored normally, it does not belong in this chest";
            if (!IsStackable(item.m_shared)) return "Stored normally, items that do not stack are never duplicated";
            if (mode == ChestMode.Discovered) return "Becomes unlimited once discovered";

            var missing = Math.Max(0, GetUnlockAmount(item.m_shared) - stored);
            return $"Store {missing} more to make it unlimited";
        }

        /// <summary>
        /// Finds the items in a chest that are closest to being unlocked in Linear mode.
        /// </summary>
        /// <param name="category">The chest's category; <see cref="ChestCategory.None"/> accepts any item.</param>
        /// <param name="inventory">The chest's inventory.</param>
        /// <param name="count">The maximum number of items to return.</param>
        /// <returns>The display name, stored amount and required amount of each item, closest first.</returns>
        public List<UnlockProgress> GetClosestUnlocks(ChestCategory category, Inventory inventory, int count)
        {
            var result = new List<UnlockProgress>();
            if (Mode != ChestMode.Linear) return result;

            var totals = inventory.CountByPrefab();
            foreach (var item in inventory.GetAllItems())
            {
                if (item.m_dropPrefab == null || !totals.TryGetValue(item.m_dropPrefab.name, out var stored)) continue;

                // Several stacks of one item share one entry.
                totals.Remove(item.m_dropPrefab.name);
                if (!CanUnlock(category, item.m_dropPrefab.name, item.m_shared)) continue;

                result.Add(new UnlockProgress(Localization.instance.Localize(item.m_shared.m_name), stored, GetUnlockAmount(item.m_shared)));
            }

            return result.OrderByDescending(progress => (float)progress.Stored / progress.Required).Take(count).ToList();
        }

        /// <summary>
        /// Gets the localized display name of an item.
        /// </summary>
        /// <param name="prefabName">The item prefab name.</param>
        /// <returns>The display name, or the prefab name if the item is unknown.</returns>
        public string GetDisplayName(string prefabName)
        {
            var shared = catalog.GetShared(prefabName) ??
                         ObjectDB.instance?.GetItemPrefab(prefabName)?.GetComponent<ItemDrop>()?.m_itemData.m_shared;
            return shared == null ? prefabName : Localization.instance.Localize(shared.m_name);
        }

        /// <summary>
        /// Checks whether storing enough of this item in a chest of the given category unlocks it in Linear mode.
        /// </summary>
        /// <param name="category">The chest category; <see cref="ChestCategory.None"/> accepts any item.</param>
        /// <param name="prefabName">The item prefab name.</param>
        /// <param name="shared">The item's shared data.</param>
        /// <returns>True if the item can still be unlocked from this chest.</returns>
        public bool CanUnlock(ChestCategory category, string prefabName, ItemDrop.ItemData.SharedData shared)
        {
            return IsStackable(shared) && BelongsIn(category, prefabName) && !progress.IsUnlocked(prefabName);
        }

        /// <summary>
        /// Gets the number of items a chest must hold to unlock the item in Linear mode.
        /// </summary>
        /// <param name="shared">The item's shared data.</param>
        /// <returns>The required amount.</returns>
        public int GetUnlockAmount(ItemDrop.ItemData.SharedData shared)
        {
            return shared.m_maxStackSize * settings.UnlockStacks.Value;
        }

        /// <summary>
        /// Unlocks an item for the whole world.
        /// </summary>
        /// <param name="prefabName">The item prefab name.</param>
        public void Unlock(string prefabName)
        {
            progress.Unlock(prefabName);
        }

        private bool IsSupplied(ChestMode mode, ChestCategory category, string prefabName, ItemDrop.ItemData.SharedData shared)
        {
            if (mode == ChestMode.Full) return true;
            if (!IsStackable(shared) || !BelongsIn(category, prefabName)) return false;

            return mode == ChestMode.Linear ? progress.IsUnlocked(prefabName) : progress.IsDiscovered(shared.m_name);
        }

        // Items that do not stack, like weapons and armor, are never duplicated in the progression modes.
        private static bool IsStackable(ItemDrop.ItemData.SharedData shared) => shared.m_maxStackSize > 1;

        private bool BelongsIn(ChestCategory category, string prefabName)
        {
            return category == ChestCategory.None || catalog.Contains(category, prefabName);
        }
    }
}
