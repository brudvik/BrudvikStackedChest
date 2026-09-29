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
        /// When enabled, chests keep their contents sorted from the top left, unlimited items first.
        /// </summary>
        public ConfigEntry<bool> SortContents { get; }

        /// <summary>
        /// When enabled, a Learn all button teaches the player the items in a chest they have not learned yet.
        /// </summary>
        public ConfigEntry<bool> LearnAll { get; }

        /// <summary>
        /// When enabled, the Learn all button also adds trophies to the player's trophy list.
        /// </summary>
        public ConfigEntry<bool> LearnTrophies { get; }

        /// <summary>
        /// When enabled, the front of a chest shows whether it is empty and how full it is.
        /// </summary>
        public ConfigEntry<bool> ShowIndicators { get; }

        /// <summary>
        /// When enabled, the contents of the chest under the crosshair are shown as item icons.
        /// </summary>
        public ConfigEntry<bool> ShowHoverPanel { get; }

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

            SortContents = config.BindConfig("General", "SortContents", true,
                "Keep the chest contents sorted from the top left: unlimited items first, then items stored by players, " +
                "each by item type and name. Turn off to arrange the chests yourself; new items are then placed in the " +
                "first free slot from the top.",
                synced: true);

            LearnAll = config.BindConfig("General", "LearnAll", true,
                "Show a Learn all button on chests that hold items the player has not learned yet. Learning an item " +
                "unlocks the recipes and build pieces that need it, as if the player had picked it up.",
                synced: true);

            LearnTrophies = config.BindConfig("General", "LearnTrophies", true,
                "Let the Learn all button learn trophies as well, which adds them to the player's trophy list.",
                synced: true);

            ShowIndicators = config.BindConfig("Display", "ShowIndicators", true,
                "Grey out the icon on the front of empty chests and show bars under it: how many slots are used, and " +
                "in the Linear and Discovered modes how many of the chest's items are unlimited.", synced: false);

            ShowHoverPanel = config.BindConfig("Display", "ShowHoverPanel", true,
                "Show the contents of the chest you look at as item icons below the crosshair.", synced: false);

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
