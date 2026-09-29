using BrudvikStackedChest.Constants;
using BrudvikStackedChest.Helpers;
using BrudvikStackedChest.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrudvikStackedChest.Piece
{
    /// <summary>
    /// Shows the contents of the chest the player looks at, as a grid of item icons below the crosshair.
    /// </summary>
    public class ChestHoverPanel
    {
        private const int Columns = 8;
        private const int MaxIcons = 32;
        private const float CellSize = 36f;
        private const float RefreshSeconds = 0.25f;
        private const string Gold = "#FFD24D";
        private const string Infinity = "\u221E";

        private readonly ChestSupply supply;
        private readonly Func<Container?, CustomPieceExtended?> findPiece;
        private readonly Func<bool> isEnabled;

        private GameObject? root;
        private TextMeshProUGUI? header;
        private TextMeshProUGUI? more;
        private readonly List<Cell> cells = new();
        private Container? shown;
        private float nextRefresh;
        private string unlimitedText = Infinity;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChestHoverPanel"/> class.
        /// </summary>
        /// <param name="supply">Decides which items are unlimited.</param>
        /// <param name="findPiece">Finds the chest definition for a container, or null if it is not one of ours.</param>
        /// <param name="isEnabled">Tells whether the panel is switched on.</param>
        public ChestHoverPanel(ChestSupply supply, Func<Container?, CustomPieceExtended?> findPiece, Func<bool> isEnabled)
        {
            this.supply = supply;
            this.findPiece = findPiece;
            this.isEnabled = isEnabled;
        }

        /// <summary>
        /// Shows, refreshes or hides the panel for the chest under the crosshair. Call once per frame.
        /// </summary>
        public void Update()
        {
            var container = GetHoveredContainer();
            var piece = container == null ? null : findPiece(container);
            var inventory = container == null ? null : container.GetInventory();
            if (piece == null || inventory == null)
            {
                Hide();
                return;
            }

            if (root == null && !TryBuild()) return;

            if (container != shown || Time.time >= nextRefresh)
            {
                shown = container;
                nextRefresh = Time.time + RefreshSeconds;
                Refresh(inventory, piece.CustomPieceConfig.ItemCategory);
            }
            root!.SetActive(true);
        }

        private Container? GetHoveredContainer()
        {
            if (!isEnabled() || Hud.instance == null || InventoryGui.IsVisible() || Menu.IsVisible()) return null;

            var hover = Player.m_localPlayer == null ? null : Player.m_localPlayer.GetHoverObject();
            return hover == null ? null : hover.GetComponentInParent<Container>();
        }

        private void Hide()
        {
            shown = null;
            if (root != null) root.SetActive(false);
        }

        private void Refresh(Inventory inventory, ChestCategory category)
        {
            var mode = supply.Mode;
            var items = inventory.GetAllItems().OrderBy(item => item.m_gridPos.y).ThenBy(item => item.m_gridPos.x).ToList();
            var slots = inventory.GetWidth() * inventory.GetHeight();

            var summary = items.Count == 0 ? Texts.Get("bsc_hover_empty") : Texts.Get("bsc_hover_slots", items.Count, slots);
            if (mode != ChestMode.Full && category != ChestCategory.None)
            {
                supply.CountProgress(category, out var supplied, out var total);
                if (total > 0) summary += $"   <color={Gold}>{Texts.Get("bsc_unlimited_count", $"{supplied}/{total}")}</color>";
            }
            header!.text = summary;

            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (i >= items.Count)
                {
                    cell.Root.SetActive(false);
                    continue;
                }

                var item = items[i];
                cell.Root.SetActive(true);
                cell.Icon.sprite = item.GetIcon();
                cell.Amount.text = supply.IsSupplied(mode, category, item)
                    ? $"<color={Gold}>{unlimitedText}</color>"
                    : item.m_stack > 1 ? item.m_stack.ToString() : string.Empty;
            }

            var hidden = items.Count - cells.Count;
            more!.gameObject.SetActive(hidden > 0);
            if (hidden > 0) more.text = Texts.Get("bsc_hover_more", hidden);
        }

        private bool TryBuild()
        {
            var hud = Hud.instance;
            if (hud == null || hud.m_rootObject == null || hud.m_hoverName == null) return false;

            var font = hud.m_hoverName.font;
            if (font != null && !font.HasCharacter(Infinity[0], true, true)) unlimitedText = "MAX";

            // The HUD is recreated for every world, taking the old panel with it.
            cells.Clear();
            root = new GameObject("BrudvikStackedChest_HoverPanel", typeof(RectTransform));
            root.transform.SetParent(hud.m_rootObject.transform, false);

            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -70f);

            AddImage(root, new Color(0f, 0f, 0f, 0.6f));

            var layout = root.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var fitter = root.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            header = CreateText("Header", root.transform, font, 16f, TextAlignmentOptions.Center);

            var grid = new GameObject("Grid", typeof(RectTransform));
            grid.transform.SetParent(root.transform, false);
            var gridLayout = grid.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(CellSize, CellSize);
            gridLayout.spacing = new Vector2(2f, 2f);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = Columns;

            for (var i = 0; i < MaxIcons; i++)
            {
                cells.Add(CreateCell(grid.transform, font));
            }

            more = CreateText("More", root.transform, font, 14f, TextAlignmentOptions.Center);
            root.SetActive(false);
            return true;
        }

        private static Cell CreateCell(Transform parent, TMP_FontAsset? font)
        {
            var cell = new GameObject("Cell", typeof(RectTransform));
            cell.transform.SetParent(parent, false);
            AddImage(cell, new Color(1f, 1f, 1f, 0.08f));

            var iconObject = new GameObject("Icon", typeof(RectTransform));
            iconObject.transform.SetParent(cell.transform, false);
            Stretch((RectTransform)iconObject.transform, 3f);
            var icon = AddImage(iconObject, Color.white);
            icon.preserveAspect = true;

            var amount = CreateText("Amount", cell.transform, font, 12f, TextAlignmentOptions.BottomRight);
            Stretch((RectTransform)amount.transform, 2f);

            return new Cell(cell, icon, amount);
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, TMP_FontAsset? font, float size, TextAlignmentOptions alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);

            var text = textObject.AddComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        private static Image AddImage(GameObject target, Color color)
        {
            var image = target.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void Stretch(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        private sealed class Cell
        {
            public Cell(GameObject root, Image icon, TextMeshProUGUI amount)
            {
                Root = root;
                Icon = icon;
                Amount = amount;
            }

            public GameObject Root { get; }

            public Image Icon { get; }

            public TextMeshProUGUI Amount { get; }
        }
    }
}
