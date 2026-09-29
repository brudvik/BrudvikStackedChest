using BrudvikStackedChest.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Biome = Heightmap.Biome;

namespace BrudvikStackedChest.Helpers
{
    /// <summary>
    /// Finds the items that can be gathered in each biome: what trees, rocks, plants and creatures there drop. The
    /// lists are built once per loaded world from the game's vegetation and creature spawn data.
    /// </summary>
    public class BiomeCatalog
    {
        /// <summary>
        /// The biomes in the order the game's progression visits them.
        /// </summary>
        public static readonly Biome[] Order =
        {
            Biome.Meadows, Biome.BlackForest, Biome.Swamp, Biome.Mountain, Biome.Plains,
            Biome.Mistlands, Biome.AshLands, Biome.DeepNorth, Biome.Ocean
        };

        private const int MaxSourceDepth = 4;

        // Items found only in dungeons and other locations, whose contents are not loaded until a player is near.
        private static readonly Dictionary<string, Biome> LocationItems = new()
        {
            { "Honey", Biome.Meadows },
            { "QueenBee", Biome.Meadows },
            { "SurtlingCore", Biome.BlackForest },
            { "BoneFragments", Biome.BlackForest },
            { "TrophySkeleton", Biome.BlackForest },
            { "TrophySkeletonPoison", Biome.BlackForest },
            { "IronScrap", Biome.Swamp },
            { "WitheredBone", Biome.Swamp },
            { "TrophyDraugrElite", Biome.Swamp },
            { "Crystal", Biome.Mountain },
            { "TrophyUlv", Biome.Mountain },
            { "TrophyCultist", Biome.Mountain },
            { "Tar", Biome.Plains },
            { "TrophyGrowth", Biome.Plains },
            { "Flax", Biome.Plains },
            { "Barley", Biome.Plains },
            { "TrophyGoblinBrute", Biome.Plains },
            { "TrophyGoblinShaman", Biome.Plains },
            { "GoblinTotem", Biome.Plains },
            { "BlackCore", Biome.Mistlands },
            { "BlackMarble", Biome.Mistlands },
            { "Sap", Biome.Mistlands },
            { "JuteBlue", Biome.Mistlands },
            { "Wisp", Biome.Mistlands },
            { "RoyalJelly", Biome.Mistlands },
            { "Softtissue", Biome.Mistlands },
            { "TrophyDvergr", Biome.Mistlands },
            { "TrophySeekerBrute", Biome.Mistlands },
            { "CharredCogwheel", Biome.AshLands },
            { "CelestialFeather", Biome.AshLands },
            { "MoltenCore", Biome.AshLands },
            { "Charredskull", Biome.AshLands },
            { "FlametalOreNew", Biome.AshLands },
            { "ProustitePowder", Biome.AshLands },
            { "BellFragment", Biome.AshLands },
            { "DyrnwynBladeFragment", Biome.AshLands },
            { "DyrnwynHiltFragment", Biome.AshLands },
            { "DyrnwynTipFragment", Biome.AshLands },
            { "SulfurStone", Biome.AshLands },
            { "TrophyCharredMelee", Biome.AshLands },
            { "TrophyCharredArcher", Biome.AshLands },
            { "TrophyCharredMage", Biome.AshLands },
            { "TrophyFallenValkyrie", Biome.AshLands },
            { "Chitin", Biome.Ocean }
        };

        private static readonly IReadOnlyList<string> NoItems = new List<string>();

        private readonly PluginSettings settings;
        private ObjectDB? builtFor;
        private Dictionary<Biome, IReadOnlyList<string>> itemsByBiome = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="BiomeCatalog"/> class.
        /// </summary>
        /// <param name="settings">The plugin settings, which decide whether the lists are written to the log.</param>
        public BiomeCatalog(PluginSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Gets the items that can be gathered in a biome. Items that are only crafted are left out.
        /// </summary>
        /// <param name="biome">A single biome.</param>
        /// <returns>The item prefab names, or an empty list if the game data is not loaded yet.</returns>
        public IReadOnlyList<string> GetItems(Biome biome)
        {
            if (!EnsureBuilt()) return NoItems;

            return itemsByBiome.TryGetValue(biome, out var items) ? items : NoItems;
        }

        private bool EnsureBuilt()
        {
            var objectDb = ObjectDB.instance;
            var zoneSystem = ZoneSystem.instance;
            if (objectDb == null || zoneSystem == null || ZNetScene.instance == null || objectDb.m_items.Count == 0) return false;
            if (builtFor == objectDb) return true;

            var builder = new Builder(objectDb, settings.DumpItemLists.Value);
            builder.AddVegetation(zoneSystem);
            builder.AddSpawns();
            builder.AddLocationItems(LocationItems);
            itemsByBiome = builder.Build();
            builtFor = objectDb;
            if (settings.DumpItemLists.Value) builder.Dump();
            return true;
        }

        private sealed class Builder
        {
            // Placements covering this many biomes or more are treated as broad placements.
            private const int BroadSourceBiomes = 4;

            private readonly ObjectDB objectDb;
            private readonly bool logBroadSources;
            private readonly HashSet<string> crafted = new(StringComparer.Ordinal);
            private readonly Dictionary<Biome, HashSet<string>> found = new();
            private readonly Dictionary<string, string> sourceOf = new(StringComparer.Ordinal);
            private string currentSource = string.Empty;

            public Builder(ObjectDB objectDb, bool logBroadSources)
            {
                this.objectDb = objectDb;
                this.logBroadSources = logBroadSources;
                foreach (var recipe in objectDb.m_recipes)
                {
                    if (recipe != null && recipe.m_enabled && recipe.m_item != null) crafted.Add(recipe.m_item.name);
                }

                // Smelted, cooked and fermented items are made, even when a pot or chest happens to hold them.
                foreach (var prefab in ZNetScene.instance.m_prefabs)
                {
                    if (prefab == null) continue;

                    if (prefab.TryGetComponent(out Smelter smelter) && smelter.m_conversion != null)
                    {
                        foreach (var conversion in smelter.m_conversion) AddCrafted(conversion?.m_to);
                    }
                    if (prefab.TryGetComponent(out CookingStation cooking) && cooking.m_conversion != null)
                    {
                        foreach (var conversion in cooking.m_conversion) AddCrafted(conversion?.m_to);
                    }
                    if (prefab.TryGetComponent(out Fermenter fermenter) && fermenter.m_conversion != null)
                    {
                        foreach (var conversion in fermenter.m_conversion) AddCrafted(conversion?.m_to);
                    }
                }
            }

            private void AddCrafted(ItemDrop? item)
            {
                if (item != null) crafted.Add(item.name);
            }

            public void AddVegetation(ZoneSystem zoneSystem)
            {
                var placements = new List<Placement>();
                foreach (var vegetation in zoneSystem.m_vegetation)
                {
                    if (vegetation == null || !vegetation.m_enable || vegetation.m_prefab == null) continue;

                    placements.Add(new Placement(vegetation.m_prefab, vegetation.m_biome, $"vegetation {vegetation.m_prefab.name}",
                        $"Vegetation '{vegetation.m_name}' altBiomeParent='{vegetation.m_altBiomeParent}'"));
                }
                AddPlacements(placements);
            }

            public void AddSpawns()
            {
                var placements = new List<Placement>();

                // The spawn lists live on objects in the game scene, not on a single known prefab.
                foreach (var list in Resources.FindObjectsOfTypeAll<SpawnSystemList>())
                {
                    if (list == null || list.m_spawners == null) continue;

                    foreach (var spawn in list.m_spawners)
                    {
                        if (spawn == null || !spawn.m_enabled || spawn.m_devDisabled || spawn.m_prefab == null) continue;

                        // Spawns tied to an ongoing event are raids, not creatures that live in the biome.
                        if (!string.IsNullOrEmpty(spawn.m_requiredPersistentEvent)) continue;

                        placements.Add(new Placement(spawn.m_prefab, spawn.m_biome, $"spawn {spawn.m_prefab.name} in {list.name}",
                            $"Spawn '{spawn.m_name}' list='{list.name}' globalKey='{spawn.m_requiredGlobalKey}' environments={spawn.m_requiredEnvironments?.Count ?? 0}",
                            spawn.m_requiredGlobalKey));
                    }
                }

                // A creature that also spreads to other biomes once a boss is defeated only lives where it spawns from the start.
                var ungated = new HashSet<GameObject>(placements.Where(placement => string.IsNullOrEmpty(placement.GlobalKey)).Select(placement => placement.Prefab));
                placements.RemoveAll(placement =>
                {
                    var later = !string.IsNullOrEmpty(placement.GlobalKey) && ungated.Contains(placement.Prefab);
                    if (later && logBroadSources)
                    {
                        Jotunn.Logger.LogInfo($"Boss-gated spawn (ignored): {placement.Details} prefab={placement.Prefab.name} biomes={placement.Biomes}");
                    }
                    return later;
                });
                AddPlacements(placements);
            }

            /// <summary>
            /// Adds the drops of every placed prefab. Creatures, spawners and loot placed in many biomes at once are
            /// raids, events and shared containers rather than something that lives in those biomes, so they are
            /// left out; plain resources such as trees, rocks and plants are kept wherever they grow.
            /// </summary>
            private void AddPlacements(List<Placement> placements)
            {
                foreach (var placement in placements)
                {
                    var broad = CountBiomes(placement.Biomes) >= BroadSourceBiomes;
                    var ignored = broad && !IsPlainResource(placement.Prefab);
                    if (logBroadSources && broad)
                    {
                        Jotunn.Logger.LogInfo($"Broad biome source{(ignored ? " (ignored)" : string.Empty)}: {placement.Details} prefab={placement.Prefab.name} biomes={placement.Biomes}");
                    }
                    if (ignored) continue;

                    currentSource = placement.Source;
                    AddSources(placement.Prefab, placement.Biomes, 0);
                }
            }

            private static bool IsPlainResource(GameObject prefab)
            {
                if (prefab.GetComponent<Character>() != null || prefab.GetComponent<SpawnArea>() != null ||
                    prefab.GetComponent<CharacterDrop>() != null || prefab.GetComponent<DropOnDestroyed>() != null ||
                    prefab.GetComponent<Container>() != null) return false;

                return prefab.GetComponent<TreeBase>() != null || prefab.GetComponent<TreeLog>() != null ||
                       prefab.GetComponent<MineRock>() != null || prefab.GetComponent<MineRock5>() != null ||
                       prefab.GetComponent<Pickable>() != null || prefab.GetComponent<PickableItem>() != null ||
                       prefab.GetComponent<ItemDrop>() != null || prefab.GetComponent<Destructible>() != null;
            }

            private static int CountBiomes(Biome biomes) => Order.Count(biome => (biomes & biome) != 0);

            public void AddLocationItems(Dictionary<string, Biome> items)
            {
                currentSource = "location table";
                foreach (var pair in items)
                {
                    AddItem(pair.Key, pair.Value);
                }
            }

            /// <summary>
            /// Writes the items of every biome to the log, each with the first source that placed it there.
            /// </summary>
            public void Dump()
            {
                foreach (var biome in Order)
                {
                    var items = found.TryGetValue(biome, out var set) ? set.OrderBy(name => name, StringComparer.Ordinal).ToList() : new List<string>();
                    var described = items.Select(name => $"{name} <{sourceOf[$"{biome}|{name}"]}>");
                    Jotunn.Logger.LogInfo($"Biome {biome} ({items.Count} items): {string.Join(", ", described)}");
                }
            }

            public Dictionary<Biome, IReadOnlyList<string>> Build()
            {
                var result = new Dictionary<Biome, IReadOnlyList<string>>();
                foreach (var pair in found)
                {
                    result[pair.Key] = pair.Value.OrderBy(name => name, StringComparer.Ordinal).ToList();
                }
                return result;
            }

            private void AddSources(GameObject? prefab, Biome biome, int depth)
            {
                if (prefab == null || depth > MaxSourceDepth) return;

                AddItem(prefab.name, biome);

                if (prefab.TryGetComponent(out TreeBase tree))
                {
                    AddDrops(tree.m_dropWhenDestroyed, biome);
                    AddSources(tree.m_logPrefab, biome, depth + 1);
                }

                if (prefab.TryGetComponent(out TreeLog log))
                {
                    AddDrops(log.m_dropWhenDestroyed, biome);
                    AddSources(log.m_subLogPrefab, biome, depth + 1);
                }

                if (prefab.TryGetComponent(out MineRock rock)) AddDrops(rock.m_dropItems, biome);
                if (prefab.TryGetComponent(out MineRock5 bigRock)) AddDrops(bigRock.m_dropItems, biome);
                if (prefab.TryGetComponent(out DropOnDestroyed dropOnDestroyed)) AddDrops(dropOnDestroyed.m_dropWhenDestroyed, biome);
                if (prefab.TryGetComponent(out Destructible destructible)) AddSources(destructible.m_spawnWhenDestroyed, biome, depth + 1);

                if (prefab.TryGetComponent(out Pickable pickable))
                {
                    AddSources(pickable.m_itemPrefab, biome, depth + 1);
                    AddDrops(pickable.m_extraDrops, biome);
                }

                if (prefab.TryGetComponent(out PickableItem pickableItem))
                {
                    if (pickableItem.m_itemPrefab != null) AddItem(pickableItem.m_itemPrefab.name, biome);
                    foreach (var random in pickableItem.m_randomItemPrefabs ?? Array.Empty<PickableItem.RandomItem>())
                    {
                        if (random.m_itemPrefab != null) AddItem(random.m_itemPrefab.name, biome);
                    }
                }

                // Boss drops are rewards for progress, not something to gather.
                var isBoss = prefab.TryGetComponent(out Character character) && character.m_boss;
                if (!isBoss && prefab.TryGetComponent(out CharacterDrop characterDrop) && characterDrop.m_drops != null)
                {
                    foreach (var drop in characterDrop.m_drops)
                    {
                        if (drop?.m_prefab != null) AddItem(drop.m_prefab.name, biome);
                    }
                }

                if (prefab.TryGetComponent(out SpawnArea spawnArea) && spawnArea.m_prefabs != null)
                {
                    foreach (var spawn in spawnArea.m_prefabs)
                    {
                        AddSources(spawn?.m_prefab, biome, depth + 1);
                    }
                }
            }

            private void AddDrops(DropTable? table, Biome biome)
            {
                if (table?.m_drops == null) return;

                foreach (var drop in table.m_drops)
                {
                    if (drop.m_item != null) AddItem(drop.m_item.name, biome);
                }
            }

            private void AddItem(string name, Biome biomes)
            {
                // Drop tables can also spawn creatures and other objects that are not items.
                if (crafted.Contains(name) || objectDb.GetItemPrefab(name)?.GetComponent<ItemDrop>() == null) return;

                foreach (var biome in Order)
                {
                    if ((biomes & biome) == 0) continue;

                    if (!found.TryGetValue(biome, out var set))
                    {
                        set = new HashSet<string>(StringComparer.Ordinal);
                        found[biome] = set;
                    }
                    if (set.Add(name)) sourceOf[$"{biome}|{name}"] = currentSource;
                }
            }

            private sealed class Placement
            {
                public Placement(GameObject prefab, Biome biomes, string source, string details, string? globalKey = null)
                {
                    Prefab = prefab;
                    Biomes = biomes;
                    Source = source;
                    Details = details;
                    GlobalKey = globalKey;
                }

                public GameObject Prefab { get; }

                public Biome Biomes { get; }

                public string Source { get; }

                public string Details { get; }

                public string? GlobalKey { get; }
            }
        }
    }
}
