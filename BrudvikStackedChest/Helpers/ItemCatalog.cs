using BrudvikStackedChest.Configuration;
using BrudvikStackedChest.Constants;
using System;
using System.Collections.Generic;

namespace BrudvikStackedChest.Helpers
{
    /// <summary>
    /// Provides the automatically generated item list for each chest category.
    /// The lists are built once per loaded world and rebuilt when the configuration changes.
    /// </summary>
    public class ItemCatalog
    {
        private static readonly IReadOnlyList<string> NoItems = new List<string>();

        private readonly PluginSettings settings;
        private ObjectDB? builtFor;
        private Dictionary<ChestCategory, IReadOnlyList<string>> itemsByCategory = new();
        private Dictionary<ChestCategory, HashSet<string>> setsByCategory = new();
        private Dictionary<string, ItemDrop.ItemData.SharedData> sharedByName = new();
        private Dictionary<string, ChestCategory> categoryByName = new();
        private Dictionary<string, int> largestRequirement = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemCatalog"/> class.
        /// </summary>
        /// <param name="settings">The plugin settings with the include and exclude lists.</param>
        public ItemCatalog(PluginSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Discards the generated lists so they are rebuilt on next use.
        /// </summary>
        public void Invalidate()
        {
            builtFor = null;
        }

        /// <summary>
        /// Gets the item prefab names that belong in a chest of the given category.
        /// </summary>
        /// <param name="category">The chest category.</param>
        /// <returns>The prefab names, or an empty list if the game data is not loaded yet.</returns>
        public IReadOnlyList<string> GetItems(ChestCategory category)
        {
            if (category == ChestCategory.None || !EnsureBuilt()) return NoItems;

            return itemsByCategory.TryGetValue(category, out var items) ? items : NoItems;
        }

        /// <summary>
        /// Checks whether an item belongs in a chest of the given category.
        /// </summary>
        /// <param name="category">The chest category.</param>
        /// <param name="prefabName">The item prefab name.</param>
        /// <returns>True if the item is part of the category's generated list.</returns>
        public bool Contains(ChestCategory category, string prefabName)
        {
            return EnsureBuilt() && setsByCategory.TryGetValue(category, out var set) && set.Contains(prefabName);
        }

        /// <summary>
        /// Gets the shared item data of an item in one of the generated lists.
        /// </summary>
        /// <param name="prefabName">The item prefab name.</param>
        /// <returns>The shared item data, or null if the item is not in any list.</returns>
        public ItemDrop.ItemData.SharedData? GetShared(string prefabName)
        {
            return EnsureBuilt() && sharedByName.TryGetValue(prefabName, out var shared) ? shared : null;
        }

        /// <summary>
        /// Gets the chest an item is placed in.
        /// </summary>
        /// <param name="prefabName">The item prefab name.</param>
        /// <returns>The chest category, or <see cref="ChestCategory.None"/> if the item is in no chest.</returns>
        public ChestCategory GetCategory(string prefabName)
        {
            return EnsureBuilt() && categoryByName.TryGetValue(prefabName, out var category) ? category : ChestCategory.None;
        }

        /// <summary>
        /// Gets the largest amount of an item that a single recipe, upgrade or build piece requires.
        /// </summary>
        /// <param name="prefabName">The item prefab name.</param>
        /// <returns>The largest required amount, or 0 if nothing requires the item.</returns>
        public int GetLargestRequirement(string prefabName)
        {
            return EnsureBuilt() && largestRequirement.TryGetValue(prefabName, out var amount) ? amount : 0;
        }

        private bool EnsureBuilt()
        {
            var objectDb = ObjectDB.instance;
            var scene = ZNetScene.instance;
            if (objectDb == null || scene == null || objectDb.m_items.Count == 0) return false;
            if (builtFor == objectDb) return true;

            itemsByCategory = new ItemSorter(objectDb, scene, settings).Sort();
            setsByCategory = new Dictionary<ChestCategory, HashSet<string>>();
            sharedByName = new Dictionary<string, ItemDrop.ItemData.SharedData>();
            categoryByName = new Dictionary<string, ChestCategory>();
            foreach (var pair in itemsByCategory)
            {
                setsByCategory[pair.Key] = new HashSet<string>(pair.Value);
                foreach (var name in pair.Value)
                {
                    if (!categoryByName.ContainsKey(name)) categoryByName[name] = pair.Key;

                    var itemDrop = objectDb.GetItemPrefab(name)?.GetComponent<ItemDrop>();
                    if (itemDrop != null) sharedByName[name] = itemDrop.m_itemData.m_shared;
                }
            }

            largestRequirement = CollectLargestRequirements(objectDb);
            builtFor = objectDb;
            return true;
        }

        private static Dictionary<string, int> CollectLargestRequirements(ObjectDB objectDb)
        {
            var result = new Dictionary<string, int>();

            foreach (var recipe in objectDb.m_recipes)
            {
                if (recipe == null || recipe.m_resources == null) continue;

                var maxQuality = recipe.m_item == null ? 1 : Math.Max(1, recipe.m_item.m_itemData.m_shared.m_maxQuality);
                for (var quality = 1; quality <= maxQuality; quality++)
                {
                    AddRequirements(result, recipe.m_resources, quality);
                }
            }

            // Build pieces come from the piece tables of the building tools.
            var tables = new HashSet<PieceTable>();
            foreach (var prefab in objectDb.m_items)
            {
                var itemDrop = prefab == null ? null : prefab.GetComponent<ItemDrop>();
                var table = itemDrop == null ? null : itemDrop.m_itemData?.m_shared?.m_buildPieces;
                if (table == null || !tables.Add(table) || table.m_pieces == null) continue;

                foreach (var piecePrefab in table.m_pieces)
                {
                    var piece = piecePrefab == null ? null : piecePrefab.GetComponent<global::Piece>();
                    if (piece != null && piece.m_resources != null) AddRequirements(result, piece.m_resources, 1);
                }
            }

            return result;
        }

        private static void AddRequirements(Dictionary<string, int> result, global::Piece.Requirement[] requirements, int quality)
        {
            foreach (var requirement in requirements)
            {
                if (requirement == null || requirement.m_resItem == null) continue;

                var name = requirement.m_resItem.name;
                var amount = requirement.GetAmount(quality);
                if (!result.TryGetValue(name, out var current) || amount > current) result[name] = amount;
            }
        }
    }
}
