using BepInEx;
using BrudvikStackedChest.Commands;
using BrudvikStackedChest.Configuration;
using BrudvikStackedChest.Constants;
using BrudvikStackedChest.Events;
using BrudvikStackedChest.Extensions;
using BrudvikStackedChest.Helpers;
using BrudvikStackedChest.Models;
using BrudvikStackedChest.Patches.Containers;
using BrudvikStackedChest.Patches.Gui;
using BrudvikStackedChest.Patches.Inventories;
using BrudvikStackedChest.Patches.Players;
using BrudvikStackedChest.Piece;
using BrudvikStackedChest.Utils;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
        public const string PluginVersion = "0.3.0";

        /// <summary>
        /// List to store custom pieces (chests) added by the plugin.
        /// </summary>
        private List<CustomPieceExtended> customPieces = new();

        /// <summary>
        /// Manager to handle custom chests, initialized with the PieceManager instance and a SpriteLoader.
        /// </summary>
        private CustomChestManager customChestManager = new(PieceManager.Instance, new SpriteLoader(PluginName), PluginName);

        // Created in Awake, because the BepInEx config file is not available to field initializers.
        private PluginSettings settings = null!;
        private ItemCatalog itemCatalog = null!;
        private WorldProgress worldProgress = null!;
        private ChestSupply chestSupply = null!;
        private ChestProgressUi progressUi = null!;
        private ChestHoverPanel hoverPanel = null!;

        // Inventories do not know their container; the inventory patches need to recognize our chests.
        private readonly ConditionalWeakTable<Inventory, Container> chestInventories = new();

        /// <summary>
        /// Awake method is called when the script instance is being loaded.
        /// This method sets up event handlers, applies Harmony patches, and logs the plugin load message.
        /// </summary>
        private void Awake()
        {
            settings = new PluginSettings(Config);
            itemCatalog = new ItemCatalog(settings);
            worldProgress = new WorldProgress(PluginName);
            chestSupply = new ChestSupply(settings, itemCatalog, worldProgress);
            progressUi = new ChestProgressUi(chestSupply, FindPiece);
            hoverPanel = new ChestHoverPanel(chestSupply, FindPiece, () => settings.ShowHoverPanel.Value);

            Config.SettingChanged += (_, _) => HandleSettingsChanged();
            SynchronizationManager.OnConfigurationSynchronized += (_, _) => HandleSettingsChanged();
            worldProgress.ItemUnlocked += HandleItemUnlockedByOthers;
            CommandManager.Instance.AddConsoleCommand(new ProgressCommand(chestSupply, progressUi, () => customPieces));

            // Register a callback to add cloned items when prefabs are registered
            PrefabManager.OnPrefabsRegistered += AddClonedItems;

            // Apply Harmony patches using the plugin's GUID
            var harmony = new Harmony(PluginGUID);
            harmony.PatchAll();

            ContainerPatch.ContainerCheckForChangesPatched += HandleContainerCheckForChanges;
            ContainerPatch.ContainerChangedPatched += HandleContainerChanged;
            InventoryPatch.InventoryAddingItemPatched += HandleInventoryAddingItem;
            InventoryPatch.InventoryMoveAllPatched += HandleInventoryMoveAll;
            ContainerPatch.ContainerDropAllItemsPatched += HandleContainerDropAllItemsPatched;
            ContainerPatch.ContainerHoverTextPatched += progressUi.HandleContainerHoverText;
            InventoryGuiPatch.InventoryGridUpdatedPatched += progressUi.HandleGridUpdated;
            InventoryGuiPatch.ItemTooltipPatched += progressUi.HandleItemTooltip;
            InventoryGuiPatch.ContainerPanelUpdatedPatched += progressUi.HandleContainerPanelUpdated;
            PlayerPatch.PlayerSpawnedPatched += HandlePlayerSpawned;
            PlayerPatch.PlayerKnownItemPatched += HandlePlayerKnownItem;

            Jotunn.Logger.LogInfo($"{PluginName} v{PluginVersion} has loaded!");
        }

        private void Update()
        {
            hoverPanel?.Update();
        }

        private void HandleSettingsChanged()
        {
            itemCatalog.Invalidate();
            RegisterDiscoveries(Player.m_localPlayer);
        }

        /// <summary>
        /// Tells every player when someone else in the world makes an item unlimited. The player who did it already
        /// got a message in the middle of the screen.
        /// </summary>
        private void HandleItemUnlockedByOthers(string prefabName)
        {
            if (Player.m_localPlayer == null) return;

            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"{chestSupply.GetDisplayName(prefabName)} is now unlimited");
        }

        /// <summary>
        /// Before a chest drops its contents, removes the stacks it supplies without limit, so only items players
        /// stored themselves are dropped.
        /// </summary>
        private void HandleContainerDropAllItemsPatched(object sender, ContainerDropAllItemsPatchEvent e)
        {
            var piece = FindPiece(e?.Container);
            if (piece == null) return;

            e!.Container.RemoveSuppliedItems(piece.CustomPieceConfig.ItemCategory, chestSupply);
        }

        /// <summary>
        /// Handles the ContainerCheckForChangesPatched event.
        /// Keeps the unlimited stacks in one of our chests full and adds the unlimited items that are missing.
        /// </summary>
        private void HandleContainerCheckForChanges(object sender, ContainerCheckForChangesPatchEvent e)
        {
            var piece = FindPiece(e?.Container);
            if (piece == null) return;

            RegisterChest(e!.Container);
            ChestEffects.UpdateGlow(e.Container, piece.Color, piece.CustomPieceConfig.ItemCategory, chestSupply);
            e.Container.Restock(piece.CustomPieceConfig.ItemCategory, chestSupply, settings.SortContents.Value);
            UpdateIndicator(e.Container, piece);
        }

        /// <summary>
        /// Restocks one of our chests as soon as its contents change, so building several pieces in a row from a
        /// chest does not run it dry before the next periodic check.
        /// </summary>
        private void HandleContainerChanged(object sender, ContainerChangedPatchEvent e)
        {
            var piece = FindPiece(e?.Container);
            if (piece == null) return;

            RegisterChest(e!.Container);
            e.Container.Restock(piece.CustomPieceConfig.ItemCategory, chestSupply, settings.SortContents.Value);
            UpdateIndicator(e.Container, piece);
        }

        private void UpdateIndicator(Container container, CustomPieceExtended piece)
        {
            var inventory = container.GetInventory();
            if (inventory == null || (ZNet.instance != null && ZNet.instance.IsDedicated())) return;

            if (!container.TryGetComponent(out ChestIndicator indicator)) indicator = container.gameObject.AddComponent<ChestIndicator>();
            indicator.Refresh(inventory, piece.CustomPieceConfig.ItemCategory, chestSupply, settings.ShowIndicators.Value);
        }

        /// <summary>
        /// Lets an item put into one of our chests disappear when the chest already holds it without limit, instead
        /// of forming yet another stack.
        /// </summary>
        private void HandleInventoryAddingItem(object sender, InventoryAddingItemPatchEvent e)
        {
            if (!TryGetChest(e.Inventory, out _, out var category)) return;

            e.Absorb = chestSupply.AbsorbsDeposit(category, e.Inventory, e.Item);
        }

        /// <summary>
        /// Makes Take all on one of our chests take only the items players stored themselves.
        /// </summary>
        private void HandleInventoryMoveAll(object sender, InventoryMoveAllPatchEvent e)
        {
            if (!TryGetChest(e.FromInventory, out var container, out var category)) return;

            e.Handled = true;

            // Until the world progress is known, unlimited stacks cannot be told apart from stored items.
            if (chestSupply.IsReady) container.MoveStoredItemsTo(e.Inventory, category, chestSupply);
        }

        private void RegisterChest(Container container)
        {
            var inventory = container.GetInventory();
            if (inventory != null) chestInventories.GetValue(inventory, _ => container);
        }

        private bool TryGetChest(Inventory inventory, out Container container, out ChestCategory category)
        {
            category = ChestCategory.None;
            if (!chestInventories.TryGetValue(inventory, out container) || container == null) return false;

            var piece = FindPiece(container);
            if (piece == null) return false;

            category = piece.CustomPieceConfig.ItemCategory;
            return true;
        }

        private void HandlePlayerSpawned(object sender, PlayerSpawnedPatchEvent e)
        {
            worldProgress.RequestSync();
            RegisterDiscoveries(e.Player);
        }

        private void HandlePlayerKnownItem(object sender, PlayerKnownItemPatchEvent e)
        {
            if (settings.Mode.Value == ChestMode.Discovered) worldProgress.Discover(new[] { e.ItemToken });
        }

        /// <summary>
        /// Shares everything the player already knows, so items discovered before joining or before the mode was
        /// switched count as well.
        /// </summary>
        private void RegisterDiscoveries(Player? player)
        {
            if (player == null || settings.Mode.Value != ChestMode.Discovered) return;

            worldProgress.Discover(player.m_knownMaterial);
        }

        private CustomPieceExtended? FindPiece(Container? container)
        {
            if (container == null || container.name == null) return null;

            foreach (var piece in customPieces)
            {
                if (container.name.Contains(piece.CustomPieceConfig.Name)) return piece;
            }
            return null;
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
                    Category = ChestCategory.Wood
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
                    Category = ChestCategory.Stone
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
                    Category = ChestCategory.Metal
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
                    Rows = 10,
                    Category = ChestCategory.Food
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
                    Category = ChestCategory.Material
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
                    Category = ChestCategory.Animal
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
                    Category = ChestCategory.Seed
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
                    Category = ChestCategory.Trophy
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
                    Category = ChestCategory.Treasure
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
                    Category = ChestCategory.Tools
                }
            ));

            customPieces.Add(customChestManager.AddCustomChest(
                new CustomChestModel()
                {
                    Name = "BSArmorChest",
                    DisplayName = "Armor Chest",
                    Description = "A chest filled with armor sets from all tiers",
                    Icon = "strg_014_round.png",
                    Color = SharedUtils.ColorFromRGB(120, 80, 40, 0.8f),
                    Category = ChestCategory.Armor
                }
            ));

            customPieces.Add(customChestManager.AddCustomChest(
                new CustomChestModel()
                {
                    Name = "BSWeaponChest",
                    DisplayName = "Weapon Chest",
                    Description = "A chest filled with weapons from all biomes",
                    Icon = "strg_032_round.png",
                    Color = SharedUtils.ColorFromRGB(180, 50, 50, 0.8f),
                    Rows = 10,
                    Columns = 8,
                    Category = ChestCategory.Weapon
                }
            ));

            customPieces.Add(customChestManager.AddCustomChest(
                new CustomChestModel()
                {
                    Name = "BSPotionChest",
                    DisplayName = "Potion Chest",
                    Description = "A chest filled with meads and potions",
                    Icon = "strg_004_round.png",
                    Color = SharedUtils.ColorFromRGB(200, 100, 200, 0.8f),
                    Category = ChestCategory.Potion
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

