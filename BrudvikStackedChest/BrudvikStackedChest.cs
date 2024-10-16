using BepInEx;
using BrudvikStackedChest.Events;
using BrudvikStackedChest.Extensions;
using BrudvikStackedChest.Helpers;
using BrudvikStackedChest.Models;
using BrudvikStackedChest.Patches.Containers;
using BrudvikStackedChest.Piece;
using BrudvikStackedChest.Utils;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Collections.Generic;

namespace BrudvikStackedChest
{
    /// <summary>
    /// Main class for the BrudvikStackedChest plugin.
    /// This class initializes the plugin, sets up custom chests, and applies Harmony patches.
    /// </summary>
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class BrudvikStackedChest : BaseUnityPlugin
    {
        /// <summary>
        /// Constants for the plugin's GUID, name, and version.
        /// </summary>
        public const string PluginGUID = "com.jotunn.BrudvikStackedChest";
        public const string PluginName = "BrudvikStackedChest";
        public const string PluginVersion = "0.0.4";

        /// <summary>
        /// List to store custom pieces (chests) added by the plugin.
        /// </summary>
        private List<CustomPieceExtended> customPieces = new();

        /// <summary>
        /// Manager to handle custom chests, initialized with the PieceManager instance and a SpriteLoader.
        /// </summary>
        private CustomChestManager customChestManager = new(PieceManager.Instance, new SpriteLoader(PluginName), PluginName);

        /// <summary>
        /// Awake method is called when the script instance is being loaded.
        /// This method sets up event handlers, applies Harmony patches, and logs the plugin load message.
        /// </summary>
        private void Awake()
        {
            // Register a callback to add cloned items when prefabs are registered
            PrefabManager.OnPrefabsRegistered += AddClonedItems;

            // Apply Harmony patches using the plugin's GUID
            var harmony = new Harmony(PluginGUID);
            harmony.PatchAll();

            // Event handler for when a container checks for changes
            ContainerPatch.ContainerCheckForChangesPatched += HandleContainerCheckForChanges;

            // Event handler for when a container is about to drop all items
            ContainerPatch.ContainerDropAllItemsPatched += HandleContainerDropAllItemsPatched;

            Jotunn.Logger.LogInfo($"{PluginName} v{PluginVersion} has loaded!");
        }

        /// <summary>
        /// This method handles the ContainerDropAllItemsPatched event.
        /// It is triggered when the DropAllItems method of a Container is called.
        /// </summary>
        private void HandleContainerDropAllItemsPatched(object sender, ContainerDropAllItemsPatchEvent e)
        {
            // Check if the event argument or the container's name is null.
            if (e?.Container?.name == null) return;

            // Iterate through the list of custom pieces.
            foreach (var piece in customPieces)
            {
                // Check if the container's name contains the name of the custom piece.
                if (e.Container.name.Contains(piece.CustomPieceConfig.Name))
                {
                    // If a match is found, empty the container's inventory.
                    e.Container.EmptyChest();
                }
            }
        }

        /// <summary>
        /// Handles the ContainerCheckForChangesPatched event.
        /// Ensures the container has the correct number of items and refills items to the maximum amount if necessary.
        /// </summary>
        private void HandleContainerCheckForChanges(object sender, ContainerCheckForChangesPatchEvent e)
        {
            if (e?.Container?.name == null) return;

            foreach (var piece in customPieces)
            {
                if (e.Container.name.Contains(piece.CustomPieceConfig.Name))
                {
                    if (e.Container.GetInventoryCount() < piece.CustomPieceConfig.SpawnItems.Count)
                    {
                        e.Container.SpawnInitialItems(piece.CustomPieceConfig.SpawnItems);
                    }
                    e.Container.RefillItemsToMax();
                }
            }
        }

        /// <summary>
        /// Adds cloned items to the custom pieces list.
        /// This method creates a new custom chest model for various prefabs and adds it to the custom pieces list.
        /// </summary>
        private void AddClonedItems()
        {
            customPieces.Add(customChestManager.AddCustomChest(
                new CustomChestModel()
                {
                    Name = "BSWoodChest",
                    DisplayName = "Wood Chest",
                    Description = "A chest filled with wood materials",
                    Icon = "strg_049_round.png",
                    Color = SharedUtils.ColorFromRGB(0, 0, 0, 0.8f),
                    SpawnItems = new List<string>
                    {
                        "Wood",
                        "FineWood",
                        "RoundLog",
                        "Blackwood",
                        "Coal",
                        "AncientBark",
                        "Root",
                        "YggdrasilWood",
                        "ElderBark"
                    }
                }
            ));

            customPieces.Add(customChestManager.AddCustomChest(
                new CustomChestModel()
                {
                    Name = "BSStoneChest",
                    DisplayName = "Stone Chest",
                    Description = "A chest filled with stone materials",
                    Icon = "strg_009_round.png",
                    Color = SharedUtils.ColorFromRGB(135, 135, 135, 0.8f),
                    SpawnItems = new List<string>
                    {
                        "Flint",
                        "Obsidian",
                        "BlackMarble",
                        "Stone",
                        "Grausten",
                        "SharpeningStone",
                        "CeramicPlate"
                    }
                }
            ));

            customPieces.Add(customChestManager.AddCustomChest(
                new CustomChestModel()
                {
                    Name = "BSMetalChest",
                    DisplayName = "Metal Chest",
                    Description = "A chest filled with metal materials",
                    Icon = "strg_082_round.png",
                    Color = SharedUtils.ColorFromRGB(163, 34, 24, 0.8f),
                    SpawnItems = new List<string>
                    {
                        "Iron",
                        "IronNails",
                        "Bronze",
                        "BronzeNails",
                        "Silver",
                        "BlackMetal",
                        "Tin",
                        "Copper",
                        "Chain",
                        "CharredCogwheel",
                        "Flametal",
                        "FlametalNew",
                        "MechanicalSpring"
                    }
                }
            ));

            customPieces.Add(customChestManager.AddCustomChest(
                new CustomChestModel()
                {
                    Name = "BSFoodChest",
                    DisplayName = "Food Chest",
                    Description = "A chest filled with food ingredients",
                    Icon = "strg_046_round.png",
                    Color = SharedUtils.ColorFromRGB(112, 81, 44, 0.8f),
                    SpawnItems = new List<string>
                    {
                        "AskBladder",
                        "Barley",
                        "BarleyFlour",
                        "Bilebag",
                        "Bloodbag",
                        "Bread",
                        "BreadDough",
                        "Blueberries",
                        "Carrot",
                        "ChickenMeat",
                        "Cloudberry",
                        "Dandelion",
                        "DeerMeat",
                        "Entrails",
                        "Flax",
                        "FishRaw",
                        "HareMeat",
                        "Honey",
                        "LoxMeat",
                        "MashedMeat",
                        "MorgenHeart",
                        "Mushroom",
                        "MushroomBlue",
                        "MushroomJotunnPuffs",
                        "MushroomMagecap",
                        "MushroomSmokePuff",
                        "Onion",
                        "Pukeberries",
                        "QueensJam",
                        "Raspberry",
                        "RawMeat",
                        "RoyalJelly",
                        "Salad",
                        "Sausages",
                        "SerpentMeat",
                        "Thistle",
                        "Turnip",
                        "Vineberry",
                        "VoltureEgg",
                        "VoltureMeat",
                        "WolfMeat",
                        "YmirRemains"
                    }
                }
            ));

            customPieces.Add(customChestManager.AddCustomChest(
                new CustomChestModel()
                {
                    Name = "BSMaterialChest",
                    DisplayName = "Material Chest",
                    Description = "A chest filled with various materials",
                    Icon = "strg_088_round.png",
                    Color = SharedUtils.ColorFromRGB(44, 47, 112, 0.8f),
                    SpawnItems = new List<string>
                    {
                        "BlackCore",
                        "CharcoalResin",
                        "Crystal",
                        "Eitr",
                        "Guck",
                        "JuteBlue",
                        "JuteRed",
                        "LinenThread",
                        "Mandible",
                        "MoltenCore",
                        "Needle",
                        "Ooze",
                        "QueenBee",
                        "QueenDrop",
                        "RefinedEitr",
                        "Resin",
                        "Sap",
                        "ShieldCore",
                        "Softtissue",
                        "SurtlingCore",
                        "Tar",
                        "Thunderstone",
                        "YagluthDrop",
                        "Wisp"
                    }
                }
            ));

            customPieces.Add(customChestManager.AddCustomChest(
                new CustomChestModel()
                {
                    Name = "BSAnimalChest",
                    DisplayName = "Animal Chest",
                    Description = "A chest filled with various animal materials",
                    Icon = "strg_012_round.png",
                    Color = SharedUtils.ColorFromRGB(181, 178, 27, 0.8f),
                    SpawnItems = new List<string>
                    {
                        "AskHide",
                        "BoneFragments",
                        "Carapace",
                        "CelestialFeather",
                        "CharredBone",
                        "Charredskull",
                        "Chitin",
                        "DeerHide",
                        "DragonTear",
                        "Feathers",
                        "FistFenrirClaw",
                        "GreydwarfEye",
                        "LeatherScraps",
                        "LoxPelt",
                        "ScaleHide",
                        "SerpentScale",
                        "TrollHide",
                        "WolfFang",
                        "WolfPelt",
                        "WitheredBone"
                    }
                }
            ));

            customPieces.Add(customChestManager.AddCustomChest(
                new CustomChestModel()
                {
                    Name = "BSSeedChest",
                    DisplayName = "Seed Chest",
                    Description = "A chest filled with various seeds",
                    Icon = "strg_029_round.png",
                    Color = SharedUtils.ColorFromRGB(27, 181, 89, 0.8f),
                    SpawnItems = new List<string>
                    {
                        "Acorn",
                        "AncientSeed",
                        "BeechSeeds",
                        "BirchSeeds",
                        "CarrotSeeds",
                        "OnionSeeds",
                        "PineCone",
                        "TurnipSeeds",
                        "VineberrySeeds"
                    }
                }
            ));

            customPieces.Add(customChestManager.AddCustomChest(
                new CustomChestModel()
                {
                    Name = "BSTrophyChest",
                    DisplayName = "Trophy Chest",
                    Description = "A chest filled with trophies",
                    Icon = "strg_091_round.png",
                    Color = SharedUtils.ColorFromRGB(21, 122, 117, 0.8f),
                    SpawnItems = new List<string>
                    {
                        "TrophyAbomination",
                        "TrophyAsksvin",
                        "TrophyBlob",
                        "TrophyBoar",
                        "TrophyBonemawSerpent",
                        "TrophyBonemass",
                        "TrophyCharredArcher",
                        "TrophyCharredMage",
                        "TrophyCharredMelee",
                        "TrophyCultist",
                        "TrophyCultist_Hildir",
                        "TrophyDeathsquito",
                        "TrophyDeer",
                        "TrophyDragonQueen",
                        "TrophyDraugr",
                        "TrophyDraugrElite",
                        "TrophyDraugrFem",
                        "TrophyDvergr",
                        "TrophyEikthyr",
                        "TrophyFallenValkyrie",
                        "TrophyFenring",
                        "TrophyFish",
                        "TrophyFader",
                        "TrophyForestTroll",
                        "TrophyFrostTroll",
                        "TrophyFuling",
                        "TrophyFulingBerserker",
                        "TrophyFulingShaman",
                        "TrophyGjall",
                        "TrophyGoblin",
                        "TrophyGoblinBrute",
                        "TrophyGoblinBruteBrosBrute",
                        "TrophyGoblinBruteBrosShaman",
                        "TrophyGoblinKing",
                        "TrophyGreydwarf",
                        "TrophyGreydwarfBrute",
                        "TrophyGreydwarfShaman",
                        "TrophyGrowth",
                        "TrophyHare",
                        "TrophyHatchling",
                        "TrophyHildir",
                        "TrophyLeech",
                        "TrophyLox",
                        "TrophyMorgen",
                        "TrophyNeck",
                        "TrophySeeker",
                        "TrophySeekerBrute",
                        "TrophySeekerQueen",
                        "TrophySerpent",
                        "TrophySGolem",
                        "TrophySkeleton",
                        "TrophySkeletonHildir",
                        "TrophySkeletonPoison",
                        "TrophySurtling",
                        "TrophyTick",
                        "TrophyTroll",
                        "TrophyUlv",
                        "TrophyVolture",
                        "TrophyWraith",
                        "TrophyWolf"
                    }
                }
            ));

            customPieces.Add(customChestManager.AddCustomChest(
                new CustomChestModel()
                {
                    Name = "BSTreasureChest",
                    DisplayName = "Treasure Chest",
                    Description = "A chest filled with treasures and riches",
                    Icon = "strg_098_round.png",
                    Color = SharedUtils.ColorFromRGB(228, 237, 95, 0.8f),
                    SpawnItems = new List<string>
                    {
                        "Amber",
                        "AmberPearl",
                        "Coins",
                        "Ruby",
                        "SilverNecklace"
                    }
                }
            ));

            customPieces.Add(customChestManager.AddCustomChest(
                new CustomChestModel()
                {
                    Name = "BSToolsChest",
                    DisplayName = "Tools Chest",
                    Description = "A chest filled with tools",
                    Icon = "strg_039_round.png",
                    Color = SharedUtils.ColorFromRGB(51, 21, 122, 0.8f),
                    SpawnItems = new List<string>
                    {
                        "BarberKit",
                        "BeltStrength",
                        "CryptKey",
                        "FishingBait",
                        "FishingBaitAshlands",
                        "FishingBaitCave",
                        "FishingBaitDeepNorth",
                        "FishingBaitForest",
                        "FishingBaitMistlands",
                        "FishingBaitOcean",
                        "FishingBaitPlains",
                        "FishingBaitSwamp",
                        "FishingRod",
                        "Lantern",
                        "SpearChitin",
                        "Wishbone",
                        "Wisp"
                    }
                }
            ));

            customPieces.Add(customChestManager.AddCustomChest(
                new CustomChestModel()
                {
                    Name = "BSEmptyChest",
                    DisplayName = "Everlasting Chest",
                    Description = "A empty chest, add items to make them last forever",
                    Icon = "strg_010_round.png",
                    Color = SharedUtils.ColorFromRGB(45, 45, 79, 0.8f)
                }
            ));

            // Unregister the callback to prevent duplicate items
            PrefabManager.OnPrefabsRegistered -= AddClonedItems;
        }

    }
}

