using BrudvikStackedChest.Configuration;
using BrudvikStackedChest.Constants;
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

        private bool EnsureBuilt()
        {
            var objectDb = ObjectDB.instance;
            var scene = ZNetScene.instance;
            if (objectDb == null || scene == null || objectDb.m_items.Count == 0) return false;
            if (builtFor == objectDb) return true;

            itemsByCategory = new ItemSorter(objectDb, scene, settings).Sort();
            setsByCategory = new Dictionary<ChestCategory, HashSet<string>>();
            sharedByName = new Dictionary<string, ItemDrop.ItemData.SharedData>();
            foreach (var pair in itemsByCategory)
            {
                setsByCategory[pair.Key] = new HashSet<string>(pair.Value);
                foreach (var name in pair.Value)
                {
                    var itemDrop = objectDb.GetItemPrefab(name)?.GetComponent<ItemDrop>();
                    if (itemDrop != null) sharedByName[name] = itemDrop.m_itemData.m_shared;
                }
            }

            builtFor = objectDb;
            return true;
        }
    }
}
