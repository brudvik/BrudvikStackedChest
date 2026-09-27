using BrudvikStackedChest.Configuration;
using BrudvikStackedChest.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ItemType = ItemDrop.ItemData.ItemType;
using SharedData = ItemDrop.ItemData.SharedData;

namespace BrudvikStackedChest.Helpers
{
    /// <summary>
    /// Sorts every obtainable item into a chest category, based on the item type and on where the
    /// item comes from in the game (recipes, smelters, trees, rocks, creatures and so on).
    /// </summary>
    internal class ItemSorter
    {
        // Metal smelters are the only smelters that burn coal, the kiln and the refineries do not.
        private const string MetalSmelterFuel = "Coal";

        private static readonly HashSet<ChestCategory> RawMaterialCategories = new()
        {
            ChestCategory.Metal, ChestCategory.Wood, ChestCategory.Stone
        };

        private static readonly HashSet<ChestCategory> FoodCategory = new() { ChestCategory.Food };

        // Items the rules place in a surprising chest, because of where the game happens to drop them.
        // The Include setting still wins over these.
        private static readonly Dictionary<string, ChestCategory> BuiltInPlacements = new()
        {
            { "Coal", ChestCategory.Wood },
            { "Obsidian", ChestCategory.Stone },
            { "Crystal", ChestCategory.Stone },
            { "SulfurStone", ChestCategory.Stone },
            { "StoneRock", ChestCategory.Stone },
            { "Chain", ChestCategory.Metal },
            { "Feathers", ChestCategory.Animal },
            { "LeatherScraps", ChestCategory.Animal },
            { "WitheredBone", ChestCategory.Animal },
            { "AsksvinCarrionNeck", ChestCategory.Animal },
            { "AsksvinCarrionPelvic", ChestCategory.Animal },
            { "AsksvinCarrionRibcage", ChestCategory.Animal },
            { "AsksvinCarrionSkull", ChestCategory.Animal },
            { "HardAntler", ChestCategory.Animal },
            { "WolfHairBundle", ChestCategory.Animal },
            { "SurtlingCore", ChestCategory.Material },
            { "Ectoplasm", ChestCategory.Material },
            { "Flax", ChestCategory.Material },
            { "Dandelion", ChestCategory.Material },
            { "JuteRed", ChestCategory.Material },
            { "Tar", ChestCategory.Material },
            { "DyrnwynHiltFragment", ChestCategory.Material },
            { "MoldArmorGoldChest", ChestCategory.Material },
            { "MoldArmorGoldHelmet", ChestCategory.Material },
            { "MoldArmorGoldLegs", ChestCategory.Material },
            { "MoldArmormediumChest", ChestCategory.Material },
            { "MoldArmorMediumHelmet", ChestCategory.Material },
            { "MoldArmorMediumLegs", ChestCategory.Material },
            { "MoldArmorMageChest", ChestCategory.Material },
            { "MoldArmorMageHelmet", ChestCategory.Material },
            { "MoldArmorMageLegs", ChestCategory.Material },
            { "MoldKeys", ChestCategory.Material },
            { "GemstoneRed", ChestCategory.Treasure },
            { "GemstoneBlue", ChestCategory.Treasure },
            { "GemstoneGreen", ChestCategory.Treasure },
            { "QueenDrop", ChestCategory.Treasure },
            { "FaderDrop", ChestCategory.Treasure },
            { "FrozenKingDrop", ChestCategory.Treasure }
        };

        private readonly ObjectDB objectDb;
        private readonly ZNetScene scene;
        private readonly PluginSettings settings;

        private readonly Dictionary<string, SharedData> items = new();
        private readonly HashSet<string> obtainable = new();
        private readonly HashSet<string> crafted = new();
        private readonly Dictionary<string, HashSet<string>> ingredientsOf = new();
        private readonly Dictionary<string, HashSet<string>> usedIn = new();
        private readonly List<KeyValuePair<string, string>> smelterConversions = new();
        private readonly HashSet<string> metalSmelted = new();
        private readonly HashSet<string> fermented = new();
        private readonly HashSet<string> cooked = new();
        private readonly HashSet<string> treeDrops = new();
        private readonly HashSet<string> rockDrops = new();
        private readonly HashSet<string> creatureDrops = new();
        private readonly HashSet<string> plantRequirements = new();
        private readonly HashSet<string> fishingAmmoTypes = new();
        private readonly Dictionary<string, ChestCategory> assigned = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemSorter"/> class.
        /// </summary>
        /// <param name="objectDb">The loaded item database.</param>
        /// <param name="scene">The loaded network scene with all world prefabs.</param>
        /// <param name="settings">The plugin settings with the include and exclude lists.</param>
        public ItemSorter(ObjectDB objectDb, ZNetScene scene, PluginSettings settings)
        {
            this.objectDb = objectDb;
            this.scene = scene;
            this.settings = settings;
        }

        /// <summary>
        /// Sorts all items and returns the prefab names per chest category.
        /// </summary>
        /// <returns>The sorted prefab names per chest category.</returns>
        public Dictionary<ChestCategory, IReadOnlyList<string>> Sort()
        {
            CollectItems();
            CollectRecipes();
            CollectWorldSources();

            var unobtainable = new List<string>();
            var candidates = SelectCandidates(unobtainable);

            ApplyIncludes();
            ApplyBuiltInPlacements(candidates);
            AssignEach(candidates, ByItemType);
            AssignEach(candidates, (_, shared) => shared.m_value > 0 ? ChestCategory.Treasure : ChestCategory.None);
            AssignEach(candidates, ByConsumable);
            AssignEach(candidates, BySeed);
            AssignEach(candidates, ByRawMaterialSource);
            Propagate(candidates, RawMaterialCategories, _ => ChestCategory.None);
            AssignEach(candidates, (name, shared) =>
                shared.m_itemType == ItemType.Material && creatureDrops.Contains(name) ? ChestCategory.Animal : ChestCategory.None);
            Propagate(candidates, FoodCategory, name => IsFoodIngredient(name) ? ChestCategory.Food : ChestCategory.None);
            AssignEach(candidates, ByRemainingType);

            var result = BuildLists();
            Jotunn.Logger.LogInfo($"Sorted {result.Values.Sum(list => list.Count)} items into {result.Count} chest categories.");

            if (settings.DumpItemLists.Value)
            {
                Dump(result, candidates, unobtainable);
            }

            return result;
        }

        private void CollectItems()
        {
            foreach (var prefab in objectDb.m_items)
            {
                if (prefab == null) continue;

                var itemDrop = prefab.GetComponent<ItemDrop>();
                var shared = itemDrop == null ? null : itemDrop.m_itemData?.m_shared;
                if (shared == null) continue;

                items[prefab.name] = shared;

                if (!string.IsNullOrEmpty(shared.m_ammoType) &&
                    (shared.m_skillType == Skills.SkillType.Fishing || IsFishingRod(shared)))
                {
                    fishingAmmoTypes.Add(shared.m_ammoType);
                }
            }
        }

        private void CollectRecipes()
        {
            foreach (var recipe in objectDb.m_recipes)
            {
                if (recipe == null || !recipe.m_enabled || recipe.m_item == null) continue;

                var output = recipe.m_item.name;
                crafted.Add(output);
                obtainable.Add(output);

                if (!ingredientsOf.TryGetValue(output, out var ingredients))
                {
                    ingredients = new HashSet<string>();
                    ingredientsOf[output] = ingredients;
                }

                if (recipe.m_resources == null) continue;

                foreach (var requirement in recipe.m_resources)
                {
                    if (requirement?.m_resItem == null) continue;

                    var ingredient = requirement.m_resItem.name;
                    ingredients.Add(ingredient);

                    if (!usedIn.TryGetValue(ingredient, out var outputs))
                    {
                        outputs = new HashSet<string>();
                        usedIn[ingredient] = outputs;
                    }
                    outputs.Add(output);
                }
            }
        }

        private void CollectWorldSources()
        {
            foreach (var prefab in scene.m_prefabs)
            {
                if (prefab == null) continue;

                if (prefab.TryGetComponent(out TreeBase tree)) AddDrops(tree.m_dropWhenDestroyed, treeDrops);
                if (prefab.TryGetComponent(out TreeLog log)) AddDrops(log.m_dropWhenDestroyed, treeDrops);
                if (prefab.TryGetComponent(out MineRock rock)) AddDrops(rock.m_dropItems, rockDrops);
                if (prefab.TryGetComponent(out MineRock5 bigRock)) AddDrops(bigRock.m_dropItems, rockDrops);
                if (prefab.TryGetComponent(out DropOnDestroyed dropOnDestroyed)) AddDrops(dropOnDestroyed.m_dropWhenDestroyed, null);
                if (prefab.TryGetComponent(out Container container)) AddDrops(container.m_defaultItems, null);
                if (prefab.TryGetComponent(out LootSpawner lootSpawner)) AddDrops(lootSpawner.m_items, null);
                if (prefab.TryGetComponent(out Beehive beehive)) AddSource(beehive.m_honeyItem, null);
                if (prefab.TryGetComponent(out SapCollector sapCollector)) AddSource(sapCollector.m_spawnItem, null);
                if (prefab.TryGetComponent(out Procreation procreation)) AddSource(procreation.m_offspring, null);

                if (prefab.TryGetComponent(out Pickable pickable))
                {
                    AddSource(pickable.m_itemPrefab, null);
                    AddDrops(pickable.m_extraDrops, null);
                }

                if (prefab.TryGetComponent(out CharacterDrop characterDrop) && characterDrop.m_drops != null)
                {
                    // Boss drops are unique trophies of progress, not animal materials.
                    var isBoss = prefab.TryGetComponent(out Character character) && character.m_boss;
                    foreach (var drop in characterDrop.m_drops)
                    {
                        AddSource(drop?.m_prefab, isBoss ? null : creatureDrops);
                    }
                }

                if (prefab.TryGetComponent(out Trader trader) && trader.m_items != null)
                {
                    foreach (var tradeItem in trader.m_items)
                    {
                        AddSource(tradeItem?.m_prefab, null);
                    }
                }

                if (prefab.TryGetComponent(out Smelter smelter)) CollectSmelter(smelter);
                if (prefab.TryGetComponent(out Fermenter fermenter)) CollectFermenter(fermenter);
                if (prefab.TryGetComponent(out CookingStation cookingStation)) CollectCookingStation(cookingStation);

                if (prefab.TryGetComponent(out Plant _) && prefab.TryGetComponent(out global::Piece piece) && piece.m_resources != null)
                {
                    foreach (var requirement in piece.m_resources)
                    {
                        if (requirement?.m_resItem != null) plantRequirements.Add(requirement.m_resItem.name);
                    }
                }
            }
        }

        private void CollectSmelter(Smelter smelter)
        {
            if (smelter.m_conversion == null) return;

            var smeltsMetal = smelter.m_fuelItem != null && smelter.m_fuelItem.name == MetalSmelterFuel;
            foreach (var conversion in smelter.m_conversion)
            {
                if (conversion?.m_from == null || conversion.m_to == null) continue;

                AddSource(conversion.m_to, null);
                smelterConversions.Add(new KeyValuePair<string, string>(conversion.m_from.name, conversion.m_to.name));

                if (smeltsMetal)
                {
                    metalSmelted.Add(conversion.m_from.name);
                    metalSmelted.Add(conversion.m_to.name);
                }
            }
        }

        private void CollectFermenter(Fermenter fermenter)
        {
            if (fermenter.m_conversion == null) return;

            foreach (var conversion in fermenter.m_conversion)
            {
                if (conversion?.m_from == null || conversion.m_to == null) continue;

                AddSource(conversion.m_to, null);
                if (IsConsumable(conversion.m_to.name))
                {
                    fermented.Add(conversion.m_from.name);
                    fermented.Add(conversion.m_to.name);
                }
            }
        }

        private void CollectCookingStation(CookingStation cookingStation)
        {
            if (cookingStation.m_conversion == null) return;

            foreach (var conversion in cookingStation.m_conversion)
            {
                if (conversion?.m_from == null || conversion.m_to == null) continue;

                AddSource(conversion.m_to, null);
                if (IsConsumable(conversion.m_to.name))
                {
                    cooked.Add(conversion.m_from.name);
                    cooked.Add(conversion.m_to.name);
                }
            }
        }

        private void AddDrops(DropTable? table, HashSet<string>? source)
        {
            if (table?.m_drops == null) return;

            foreach (var drop in table.m_drops)
            {
                AddSource(drop.m_item, source);
            }
        }

        private void AddSource(Component? component, HashSet<string>? source)
        {
            if (component != null) AddSource(component.gameObject, source);
        }

        private void AddSource(GameObject? prefab, HashSet<string>? source)
        {
            // Drop tables can also spawn creatures and other non-item objects.
            if (prefab == null || !items.ContainsKey(prefab.name)) return;

            obtainable.Add(prefab.name);
            source?.Add(prefab.name);
        }

        private List<string> SelectCandidates(List<string> unobtainable)
        {
            // Several prefabs share one display name (unused variants, creature gear); keep one of them.
            var byDisplayName = new Dictionary<string, string>();
            foreach (var pair in items)
            {
                var shared = pair.Value;
                if (!IsRealItem(shared)) continue;

                if (!obtainable.Contains(pair.Key) && shared.m_itemType != ItemType.Fish)
                {
                    unobtainable.Add(pair.Key);
                    continue;
                }

                if (!byDisplayName.TryGetValue(shared.m_name, out var current) || IsPreferred(pair.Key, current))
                {
                    byDisplayName[shared.m_name] = pair.Key;
                }
            }

            return byDisplayName.Values.ToList();
        }

        private static bool IsRealItem(SharedData shared)
        {
            // Creature attacks and test items have no localization token or no icon.
            return shared.m_name != null && shared.m_name.StartsWith("$") &&
                   string.IsNullOrEmpty(shared.m_dlc) &&
                   shared.m_icons != null && shared.m_icons.Length > 0 && shared.m_icons[0] != null &&
                   shared.m_itemType != ItemType.None && shared.m_itemType != ItemType.Customization;
        }

        private bool IsPreferred(string candidate, string current)
        {
            var candidateCrafted = crafted.Contains(candidate);
            if (candidateCrafted != crafted.Contains(current)) return candidateCrafted;
            if (candidate.Length != current.Length) return candidate.Length < current.Length;
            return string.CompareOrdinal(candidate, current) < 0;
        }

        private void ApplyIncludes()
        {
            foreach (ChestCategory category in Enum.GetValues(typeof(ChestCategory)))
            {
                foreach (var name in settings.GetIncluded(category))
                {
                    if (!items.ContainsKey(name))
                    {
                        Jotunn.Logger.LogWarning($"Chest.{category} Include: no item named '{name}'.");
                        continue;
                    }

                    if (!assigned.ContainsKey(name)) assigned[name] = category;
                }
            }
        }

        private void AssignEach(List<string> candidates, Func<string, SharedData, ChestCategory> rule)
        {
            foreach (var name in candidates)
            {
                if (assigned.ContainsKey(name)) continue;

                var category = rule(name, items[name]);
                if (category != ChestCategory.None) assigned[name] = category;
            }
        }

        private ChestCategory ByItemType(string name, SharedData shared)
        {
            switch (shared.m_itemType)
            {
                case ItemType.Trophy:
                    return ChestCategory.Trophy;
                case ItemType.Helmet:
                case ItemType.Chest:
                case ItemType.Legs:
                case ItemType.Shoulder:
                case ItemType.Hands:
                case ItemType.Trinket:
                    return ChestCategory.Armor;
                case ItemType.Tool:
                case ItemType.Torch:
                case ItemType.Utility:
                    return ChestCategory.Tools;
                case ItemType.Ammo:
                case ItemType.AmmoNonEquipable:
                    return shared.m_ammoType != null && fishingAmmoTypes.Contains(shared.m_ammoType)
                        ? ChestCategory.Tools
                        : ChestCategory.Weapon;
                case ItemType.OneHandedWeapon:
                case ItemType.TwoHandedWeapon:
                case ItemType.TwoHandedWeaponLeft:
                case ItemType.Bow:
                case ItemType.Shield:
                case ItemType.Attach_Atgeir:
                    return IsToolSkill(shared.m_skillType) || IsFishingRod(shared) ? ChestCategory.Tools : ChestCategory.Weapon;
                default:
                    return ChestCategory.None;
            }
        }

        private static bool IsToolSkill(Skills.SkillType skill)
        {
            return skill == Skills.SkillType.Pickaxes || skill == Skills.SkillType.Fishing || skill == Skills.SkillType.Farming;
        }

        // Recognizes the fishing rod by the float it casts, whatever skill it is set to.
        private static bool IsFishingRod(SharedData shared)
        {
            return CastsFishingFloat(shared.m_attack) || CastsFishingFloat(shared.m_secondaryAttack);
        }

        private static bool CastsFishingFloat(Attack? attack)
        {
            var projectile = attack?.m_attackProjectile;
            if (projectile == null) return false;
            if (projectile.GetComponentInChildren<FishingFloat>(true) != null) return true;

            var spawned = projectile.GetComponent<Projectile>()?.m_spawnOnHit;
            return spawned != null && spawned.GetComponentInChildren<FishingFloat>(true) != null;
        }

        private void ApplyBuiltInPlacements(List<string> candidates)
        {
            foreach (var name in candidates)
            {
                if (!assigned.ContainsKey(name) && BuiltInPlacements.TryGetValue(name, out var category)) assigned[name] = category;
            }
        }

        private ChestCategory ByConsumable(string name, SharedData shared)
        {
            if (fermented.Contains(name)) return ChestCategory.Potion;
            if (shared.m_itemType == ItemType.Fish || cooked.Contains(name)) return ChestCategory.Food;
            if (shared.m_itemType != ItemType.Consumable) return ChestCategory.None;

            return HasFoodValue(shared) ? ChestCategory.Food : ChestCategory.Potion;
        }

        private ChestCategory BySeed(string name, SharedData shared)
        {
            // Crops like turnips are also planted, but they are ingredients first and foremost.
            return plantRequirements.Contains(name) && !usedIn.ContainsKey(name) && shared.m_itemType != ItemType.Consumable
                ? ChestCategory.Seed
                : ChestCategory.None;
        }

        private ChestCategory ByRawMaterialSource(string name, SharedData shared)
        {
            if (metalSmelted.Contains(name)) return ChestCategory.Metal;
            if (shared.m_itemType != ItemType.Material) return ChestCategory.None;
            if (treeDrops.Contains(name)) return ChestCategory.Wood;
            if (rockDrops.Contains(name)) return ChestCategory.Stone;
            return ChestCategory.None;
        }

        private ChestCategory ByRemainingType(string name, SharedData shared)
        {
            switch (shared.m_itemType)
            {
                case ItemType.Material:
                    return ChestCategory.Material;
                case ItemType.Misc:
                    // Crafted curiosities are equipment like saddles and keys, the rest are found treasures.
                    return crafted.Contains(name) ? ChestCategory.Tools : ChestCategory.Treasure;
                default:
                    return ChestCategory.None;
            }
        }

        /// <summary>
        /// Repeatedly assigns items that are made from, or smelted from, items of one of the given categories.
        /// </summary>
        private void Propagate(List<string> candidates, HashSet<ChestCategory> categories, Func<string, ChestCategory> fallback)
        {
            bool changed;
            do
            {
                changed = false;
                foreach (var name in candidates)
                {
                    if (assigned.ContainsKey(name)) continue;

                    var type = items[name].m_itemType;
                    if (type != ItemType.Material && type != ItemType.Misc) continue;

                    var category = CategoryOfIngredients(name, categories);
                    if (category == ChestCategory.None) category = fallback(name);
                    if (category == ChestCategory.None) continue;

                    assigned[name] = category;
                    changed = true;
                }
            }
            while (changed);
        }

        private ChestCategory CategoryOfIngredients(string name, HashSet<ChestCategory> categories)
        {
            if (ingredientsOf.TryGetValue(name, out var ingredients) && ingredients.Count > 0)
            {
                var common = ChestCategory.None;
                foreach (var ingredient in ingredients)
                {
                    if (!assigned.TryGetValue(ingredient, out var category) || !categories.Contains(category) ||
                        (common != ChestCategory.None && common != category))
                    {
                        common = ChestCategory.None;
                        break;
                    }
                    common = category;
                }

                if (common != ChestCategory.None) return common;
            }

            foreach (var conversion in smelterConversions)
            {
                if (conversion.Value == name && assigned.TryGetValue(conversion.Key, out var category) && categories.Contains(category))
                {
                    return category;
                }
            }

            return ChestCategory.None;
        }

        private bool IsFoodIngredient(string name)
        {
            if (usedIn.TryGetValue(name, out var outputs) && outputs.Any(IsAssignedFood)) return true;

            return smelterConversions.Any(conversion => conversion.Key == name && IsAssignedFood(conversion.Value));
        }

        private bool IsAssignedFood(string name)
        {
            return assigned.TryGetValue(name, out var category) && category == ChestCategory.Food;
        }

        private bool IsConsumable(string name)
        {
            return items.TryGetValue(name, out var shared) && shared.m_itemType == ItemType.Consumable;
        }

        private static bool HasFoodValue(SharedData shared)
        {
            return shared.m_food > 0f || shared.m_foodStamina > 0f || shared.m_foodEitr > 0f;
        }

        private Dictionary<ChestCategory, IReadOnlyList<string>> BuildLists()
        {
            var result = new Dictionary<ChestCategory, IReadOnlyList<string>>();
            foreach (var group in assigned.GroupBy(pair => pair.Value, pair => pair.Key))
            {
                var excluded = settings.GetExcluded(group.Key);
                result[group.Key] = group
                    .Where(name => !excluded.Contains(name))
                    .OrderBy(name => items[name].m_itemType)
                    .ThenBy(name => items[name].m_skillType)
                    .ThenBy(name => name, StringComparer.Ordinal)
                    .ToList();
            }
            return result;
        }

        private void Dump(Dictionary<ChestCategory, IReadOnlyList<string>> result, List<string> candidates, List<string> unobtainable)
        {
            foreach (var pair in result.OrderBy(pair => pair.Key))
            {
                Jotunn.Logger.LogInfo($"[{pair.Key}] {pair.Value.Count} items: {string.Join(", ", pair.Value)}");
            }

            var unsorted = candidates.Where(name => !assigned.ContainsKey(name)).OrderBy(name => name, StringComparer.Ordinal).ToList();
            Jotunn.Logger.LogInfo($"[Unsorted] {unsorted.Count} items: {string.Join(", ", unsorted)}");

            unobtainable.Sort(StringComparer.Ordinal);
            Jotunn.Logger.LogInfo($"[Not obtainable] {unobtainable.Count} items: {string.Join(", ", unobtainable)}");
        }
    }
}
