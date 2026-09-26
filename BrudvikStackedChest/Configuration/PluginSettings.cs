using BepInEx.Configuration;
using BrudvikStackedChest.Constants;
using Jotunn.Extensions;
using System;
using System.Collections.Generic;

namespace BrudvikStackedChest.Configuration
{
    /// <summary>
    /// Binds and exposes the plugin configuration. Chest contents are server-synced so every player sees the same chests.
    /// </summary>
    public class PluginSettings
    {
        // Items the automatic sorting cannot place correctly on its own.
        private static readonly Dictionary<ChestCategory, string> DefaultIncludes = new()
        {
            { ChestCategory.Wood, "Root" },
            { ChestCategory.Stone, "Flint" },
            { ChestCategory.Seed, "AncientSeed" },
            { ChestCategory.Animal, "Chitin" },
            { ChestCategory.Material, "Softtissue" },
            { ChestCategory.Treasure, "DragonEgg" }
        };

        private readonly Dictionary<ChestCategory, ConfigEntry<string>> includes = new();
        private readonly Dictionary<ChestCategory, ConfigEntry<string>> excludes = new();

        /// <summary>
        /// When enabled, the generated item list for every chest is written to the log.
        /// </summary>
        public ConfigEntry<bool> DumpItemLists { get; }

        /// <summary>
        /// Decides which items the chests supply without limit.
        /// </summary>
        public ConfigEntry<ChestMode> Mode { get; }

        /// <summary>
        /// In <see cref="ChestMode.Linear"/> mode, the number of full stacks a chest must hold to unlock an item.
        /// </summary>
        public ConfigEntry<int> UnlockStacks { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginSettings"/> class and binds all entries.
        /// </summary>
        /// <param name="config">The plugin's configuration file.</param>
        public PluginSettings(ConfigFile config)
        {
            DumpItemLists = config.BindConfig("General", "DumpItemLists", false,
                "Write the automatically generated item list for every chest to the BepInEx log.", synced: false);

            Mode = config.BindConfig("General", "Mode", ChestMode.Full,
                "Full: every item is always available. " +
                "Linear: store a full stack of an item in a chest to make it unlimited for the whole world. " +
                "Discovered: an item is unlimited once any player in the world has discovered it. " +
                "In Linear and Discovered, items that do not stack are never duplicated. " +
                "Switching from a more generous mode removes the items the new mode does not supply.",
                synced: true);

            UnlockStacks = config.BindConfig("General", "UnlockStacks", 1,
                "Linear mode: the number of full stacks of an item a chest must hold before the item is unlocked.",
                synced: true, acceptableValues: new AcceptableValueRange<int>(1, 10));

            foreach (ChestCategory category in Enum.GetValues(typeof(ChestCategory)))
            {
                if (category == ChestCategory.None) continue;

                var section = $"Chest.{category}";
                DefaultIncludes.TryGetValue(category, out var defaultInclude);

                includes[category] = config.BindConfig(section, "Include", defaultInclude ?? string.Empty,
                    "Comma-separated item prefab names that are always placed in this chest, overriding the automatic sorting.",
                    synced: true);
                excludes[category] = config.BindConfig(section, "Exclude", string.Empty,
                    "Comma-separated item prefab names that are never placed in this chest.",
                    synced: true);
            }
        }

        /// <summary>
        /// Gets the prefab names that are forced into the chest of the given category.
        /// </summary>
        /// <param name="category">The chest category.</param>
        /// <returns>The configured prefab names.</returns>
        public IEnumerable<string> GetIncluded(ChestCategory category) => Split(includes, category);

        /// <summary>
        /// Gets the prefab names that must never appear in the chest of the given category.
        /// </summary>
        /// <param name="category">The chest category.</param>
        /// <returns>The configured prefab names.</returns>
        public HashSet<string> GetExcluded(ChestCategory category) => new(Split(excludes, category));

        private static IEnumerable<string> Split(Dictionary<ChestCategory, ConfigEntry<string>> entries, ChestCategory category)
        {
            if (!entries.TryGetValue(category, out var entry) || string.IsNullOrWhiteSpace(entry.Value)) yield break;

            foreach (var part in entry.Value.Split(','))
            {
                var name = part.Trim();
                if (name.Length > 0) yield return name;
            }
        }
    }
}
