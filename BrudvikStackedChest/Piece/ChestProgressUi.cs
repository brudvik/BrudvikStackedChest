using BrudvikStackedChest.Constants;
using BrudvikStackedChest.Events;
using BrudvikStackedChest.Extensions;
using BrudvikStackedChest.Helpers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BrudvikStackedChest.Piece
{
    /// <summary>
    /// Shows in the inventory screen and in hover texts which items are unlimited and how far the rest are from it.
    /// </summary>
    public class ChestProgressUi
    {
        private const string Gold = "#FFD24D";
        private const string Infinity = "\u221E";
        private static readonly Color ProgressBarColor = new(0.4f, 0.75f, 1f, 1f);

        private readonly ChestSupply supply;
        private readonly Func<Container?, CustomPieceExtended?> findPiece;

        private int totalsFrame = -1;
        private Inventory? totalsInventory;
        private Dictionary<string, int> totals = new();
        private TMP_FontAsset? checkedFont;
        private string unlimitedText = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChestProgressUi"/> class.
        /// </summary>
        /// <param name="supply">Decides which items are unlimited.</param>
        /// <param name="findPiece">Finds the chest definition for a container, or null if it is not one of ours.</param>
        public ChestProgressUi(ChestSupply supply, Func<Container?, CustomPieceExtended?> findPiece)
        {
            this.supply = supply;
            this.findPiece = findPiece;
        }

        /// <summary>
        /// Marks unlimited stacks with an infinity sign and shows a progress bar under items that can still be unlocked.
        /// </summary>
        public void HandleGridUpdated(object sender, InventoryGridUpdatedPatchEvent e)
        {
            if (!TryGetOpenChest(e.Grid, out var category)) return;

            var mode = supply.Mode;
            var inventory = e.Grid.m_inventory;
            var stored = GetTotals(inventory);
            foreach (var item in inventory.GetAllItems())
            {
                var element = e.Grid.GetElement(item.m_gridPos.x, item.m_gridPos.y, e.Grid.m_width);
                if (element == null) continue;

                if (supply.IsSupplied(mode, category, item))
                {
                    element.m_amount.gameObject.SetActive(true);
                    element.m_amount.text = GetUnlimitedText(element.m_amount);
                    continue;
                }

                if (mode != ChestMode.Linear || item.m_dropPrefab == null ||
                    !supply.CanUnlock(category, item.m_dropPrefab.name, item.m_shared)) continue;

                stored.TryGetValue(item.m_dropPrefab.name, out var amount);
                element.m_durability.gameObject.SetActive(true);
                element.m_durability.SetValue(Mathf.Clamp01((float)amount / supply.GetUnlockAmount(item.m_shared)));
                element.m_durability.SetColor(ProgressBarColor);
            }
        }

        /// <summary>
        /// Adds the item's status in the chest to its tooltip.
        /// </summary>
        public void HandleItemTooltip(object sender, ItemTooltipPatchEvent e)
        {
            if (!TryGetOpenChest(e.Grid, out var category)) return;

            var amount = 0;
            if (e.Item.m_dropPrefab != null) GetTotals(e.Grid.m_inventory).TryGetValue(e.Item.m_dropPrefab.name, out amount);

            var status = supply.GetStatus(category, e.Item, amount);
            e.Tooltip.Set(e.Tooltip.m_topic, $"{e.Tooltip.m_text}\n\n<color={Gold}>{status}</color>", e.Grid.m_tooltipAnchor, Vector2.zero);
        }

        /// <summary>
        /// Adds the chest's progress to the title of the open container panel.
        /// </summary>
        public void HandleContainerPanelUpdated(object sender, ContainerPanelUpdatedPatchEvent e)
        {
            var piece = findPiece(e.Gui.m_currentContainer);
            if (piece == null) return;

            var summary = GetSummary(piece.CustomPieceConfig.ItemCategory);
            if (summary == null) return;

            var suffix = $" - {summary}";
            var title = e.Gui.m_containerName.text ?? string.Empty;
            if (!title.EndsWith(suffix, StringComparison.Ordinal)) e.Gui.m_containerName.text = title + suffix;
        }

        /// <summary>
        /// Adds the chest's progress, and the items closest to being unlocked, to the chest's hover text.
        /// </summary>
        public void HandleContainerHoverText(object sender, ContainerHoverTextPatchEvent e)
        {
            var piece = findPiece(e.Container);
            if (piece == null) return;

            var category = piece.CustomPieceConfig.ItemCategory;
            var summary = GetSummary(category);
            if (summary != null)
            {
                e.Text += $"\n{summary}";
            }
            else if (category != ChestCategory.None)
            {
                e.Text += "\nStores items like a normal chest";
                return;
            }

            var inventory = e.Container.GetInventory();
            if (inventory == null) return;

            foreach (var progress in supply.GetClosestUnlocks(category, inventory, 3))
            {
                e.Text += $"\n  {progress.DisplayName} {progress.Stored}/{progress.Required}";
            }
        }

        /// <summary>
        /// Describes how many of a category's items are unlimited.
        /// </summary>
        /// <param name="category">The chest category.</param>
        /// <returns>The summary, or null if none of the category's items can become unlimited.</returns>
        public string? GetSummary(ChestCategory category)
        {
            if (category == ChestCategory.None) return null;

            supply.CountProgress(category, out var supplied, out var total);
            return total == 0 ? null : $"<color={Gold}>{supplied}/{total}</color> unlimited";
        }

        private bool TryGetOpenChest(InventoryGrid grid, out ChestCategory category)
        {
            category = ChestCategory.None;
            var gui = InventoryGui.instance;
            if (gui == null || grid != gui.m_containerGrid) return false;

            var piece = findPiece(gui.m_currentContainer);
            if (piece == null) return false;

            category = piece.CustomPieceConfig.ItemCategory;
            return true;
        }

        // The grid asks for every slot's tooltip each frame; count the chest once per frame instead.
        private Dictionary<string, int> GetTotals(Inventory inventory)
        {
            if (totalsFrame != Time.frameCount || totalsInventory != inventory)
            {
                totals = inventory.CountByPrefab();
                totalsFrame = Time.frameCount;
                totalsInventory = inventory;
            }
            return totals;
        }

        private string GetUnlimitedText(TMP_Text text)
        {
            if (text.font != checkedFont || unlimitedText.Length == 0)
            {
                checkedFont = text.font;
                var hasInfinity = text.font != null && text.font.HasCharacter(Infinity[0], true, true);
                unlimitedText = $"<color={Gold}>{(hasInfinity ? Infinity : "MAX")}</color>";
            }
            return unlimitedText;
        }
    }
}
