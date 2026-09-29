using BrudvikStackedChest.Constants;
using BrudvikStackedChest.Events;
using BrudvikStackedChest.Extensions;
using BrudvikStackedChest.Helpers;
using BrudvikStackedChest.Utils;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Biome = Heightmap.Biome;

namespace BrudvikStackedChest.Piece
{
    /// <summary>
    /// Adds a button to the player's inventory screen that opens a panel with the items that can be gathered in the
    /// biome the player is in, and how far each of them is from being unlimited.
    /// </summary>
    public class GatheringPanel
    {
        private const string ButtonName = "BrudvikStackedChest_GatheringButton";
        private const string PanelName = "BrudvikStackedChest_GatheringPanel";
        private const float PanelWidth = 640f;
        private const float PanelHeight = 620f;
        private const float ListWidth = PanelWidth - 50f;
        private const float RowHeight = 44f;
        private const float RefreshSeconds = 0.5f;
        private const string Gold = "#FFD24D";
        private const string Infinity = "\u221E";

        private static readonly Color TitleColor = new(1f, 0.63f, 0.24f, 1f);
        private static readonly Color BarColor = new(0.4f, 0.75f, 1f, 1f);
        private static readonly Color BarBackgroundColor = new(0f, 0f, 0f, 0.6f);
        private static readonly Color SubtleColor = new(0.75f, 0.75f, 0.75f, 1f);
        private static readonly Color BagColor = new(0.55f, 0.9f, 0.55f, 1f);

        private readonly ItemCatalog catalog;
        private readonly BiomeCatalog biomes;
        private readonly ChestSupply supply;
        private readonly WorldProgress progress;
        private readonly Func<ChestCategory, string?> chestName;
        private readonly Func<Sprite?> buttonIcon;
        private readonly List<Row> rows = new();

        private InventoryGui? builtFor;
        private GameObject? panel;
        private TMP_Text? title;
        private TMP_Text? summary;
        private TMP_Text? emptyText;
        private RectTransform? content;
        private TMP_FontAsset? font;
        private string unlimitedText = Infinity;
        private Biome shownBiome = Biome.None;
        private bool followPlayer = true;
        private float nextRefresh;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatheringPanel"/> class.
        /// </summary>
        /// <param name="catalog">The generated item lists per chest category.</param>
        /// <param name="biomes">The items that can be gathered per biome.</param>
        /// <param name="supply">Decides which items are unlimited.</param>
        /// <param name="progress">The world-wide progress, with the amounts stored towards unlocking items.</param>
        /// <param name="chestName">Gets the translated name of the chest for a category.</param>
        /// <param name="buttonIcon">Loads the icon for the button that opens the panel.</param>
        public GatheringPanel(ItemCatalog catalog, BiomeCatalog biomes, ChestSupply supply, WorldProgress progress,
            Func<ChestCategory, string?> chestName, Func<Sprite?> buttonIcon)
        {
            this.catalog = catalog;
            this.biomes = biomes;
            this.supply = supply;
            this.progress = progress;
            this.chestName = chestName;
            this.buttonIcon = buttonIcon;
        }

        /// <summary>
        /// Adds the button and the panel the first time the inventory screen of a world is shown.
        /// </summary>
        public void HandleInventoryShown(object sender, InventoryGuiPatchEvent e)
        {
            if (builtFor == e.Gui) return;

            builtFor = e.Gui;
            rows.Clear();
            panel = null;
            try
            {
                Build(e.Gui);
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError($"Could not add the gathering panel to the inventory screen: {ex}");
            }
        }

        /// <summary>
        /// Closes the panel when the inventory screen closes or opens one of its own panels.
        /// </summary>
        public void HandleClose(object sender, InventoryGuiPatchEvent e)
        {
            Close();
        }

        /// <summary>
        /// Refreshes the open panel. Call once per frame.
        /// </summary>
        public void Update()
        {
            if (panel == null || !panel.activeSelf || Time.time < nextRefresh) return;

            Refresh();
        }

        private void Toggle()
        {
            if (panel == null) return;

            if (panel.activeSelf) Close();
            else Open();
        }

        private void Open()
        {
            var gui = builtFor;
            if (gui == null || panel == null) return;

            if (gui.m_trophiesPanel != null && gui.m_trophiesPanel.activeSelf) gui.OnCloseTrophies();
            if (gui.m_achievementsPanel != null && gui.m_achievementsPanel.gameObject.activeSelf) gui.OnCloseAchievements();
            if (gui.m_skillsDialog != null && gui.m_skillsDialog.gameObject.activeSelf) gui.m_skillsDialog.OnClose();
            if (gui.m_textsDialog != null && gui.m_textsDialog.gameObject.activeSelf) gui.m_textsDialog.OnClose();

            followPlayer = true;
            panel.SetActive(true);
            panel.transform.SetAsLastSibling();
            Refresh();
        }

        private void Close()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void Browse(int direction)
        {
            var player = Player.m_localPlayer;
            if (player == null) return;

            var known = GetKnownBiomes(player, GetCurrentBiome(player));
            var index = known.IndexOf(shownBiome);
            followPlayer = false;
            shownBiome = known[(Math.Max(index, 0) + direction + known.Count) % known.Count];
            Refresh();
        }

        private void Refresh()
        {
            nextRefresh = Time.time + RefreshSeconds;
            var player = Player.m_localPlayer;
            if (player == null || title == null || summary == null || emptyText == null || content == null) return;

            var current = GetCurrentBiome(player);
            if (followPlayer || !GetKnownBiomes(player, current).Contains(shownBiome)) shownBiome = current;

            title.text = Texts.Localize("$biome_" + shownBiome.ToString().ToLowerInvariant());

            var entries = CollectEntries(player);
            var countable = entries.Count(entry => entry.State != EntryState.Normal);
            var unlimited = entries.Count(entry => entry.State == EntryState.Unlimited);

            var text = countable == 0 ? string.Empty : Texts.Get("bsc_unlimited_count", $"<color={Gold}>{unlimited}/{countable}</color>");
            if (shownBiome == current) text += $"    {Texts.Get("bsc_gather_here")}";
            if (!supply.IsReady) text += $"    {Texts.Get("bsc_gather_waiting")}";
            summary.text = text.Trim();

            emptyText.gameObject.SetActive(entries.Count == 0);
            while (rows.Count < entries.Count)
            {
                rows.Add(CreateRow(content, rows.Count));
            }

            for (var i = 0; i < rows.Count; i++)
            {
                if (i < entries.Count) Fill(rows[i], entries[i]);
                else rows[i].Root.SetActive(false);
            }
        }

        private List<Entry> CollectEntries(Player player)
        {
            var mode = supply.Mode;
            var bag = player.GetInventory().CountByPrefab();
            var entries = new List<Entry>();
            foreach (var name in biomes.GetItems(shownBiome))
            {
                var shared = catalog.GetShared(name);
                if (shared == null) continue;

                var entry = new Entry(shared, Texts.Localize(shared.m_name), chestName(catalog.GetCategory(name)));
                bag.TryGetValue(name, out entry.InBag);

                if (supply.IsUnlimited(name, shared))
                {
                    entry.State = EntryState.Unlimited;
                }
                else if (!ChestSupply.IsStackable(shared))
                {
                    entry.State = EntryState.Normal;
                }
                else if (mode == ChestMode.Discovered)
                {
                    entry.State = EntryState.NotDiscovered;
                }
                else
                {
                    entry.State = EntryState.Progress;
                    entry.Required = supply.GetUnlockAmount(shared);
                    entry.Stored = Math.Min(progress.GetBestStored(name), entry.Required);
                }

                entries.Add(entry);
            }

            return entries
                .OrderBy(entry => (int)entry.State)
                .ThenByDescending(entry => entry.Required == 0 ? 0f : (float)entry.Stored / entry.Required)
                .ThenBy(entry => entry.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        private void Fill(Row row, Entry entry)
        {
            row.Root.SetActive(true);
            row.Group.alpha = entry.State == EntryState.Normal ? 0.45f : 1f;
            row.Icon.sprite = entry.Shared.m_icons != null && entry.Shared.m_icons.Length > 0 ? entry.Shared.m_icons[0] : null;
            row.Icon.enabled = row.Icon.sprite != null;
            row.Name.text = entry.DisplayName;
            row.Chest.text = entry.ChestName ?? string.Empty;
            row.Bag.text = entry.InBag > 0 ? Texts.Get("bsc_gather_bag", entry.InBag) : string.Empty;

            var showBar = entry.State == EntryState.Progress;
            row.Bar.SetActive(showBar);
            row.Unlimited.gameObject.SetActive(entry.State == EntryState.Unlimited);
            row.Unlimited.text = $"<color={Gold}>{unlimitedText}</color>";

            switch (entry.State)
            {
                case EntryState.Progress:
                    var fraction = entry.Required == 0 ? 0f : Mathf.Clamp01((float)entry.Stored / entry.Required);
                    row.Fill.anchorMax = new Vector2(fraction, 1f);
                    row.Amount.text = $"{entry.Stored} / {entry.Required}";
                    row.Status.text = Texts.Get("bsc_gather_missing", entry.Required - entry.Stored);
                    break;
                case EntryState.NotDiscovered:
                    row.Status.text = Texts.Get("bsc_gather_not_discovered");
                    break;
                case EntryState.Normal:
                    row.Status.text = Texts.Get("bsc_status_stored");
                    break;
                default:
                    row.Status.text = Texts.Get("bsc_status_unlimited");
                    break;
            }
        }

        private static Biome GetCurrentBiome(Player player)
        {
            var biome = player.GetCurrentBiome();
            return Array.IndexOf(BiomeCatalog.Order, biome) >= 0 ? biome : Biome.Meadows;
        }

        /// <summary>
        /// Gets the biomes the player has visited. The game stores them under names of its own; when none of them can
        /// be recognized, every biome is offered.
        /// </summary>
        private static List<Biome> GetKnownBiomes(Player player, Biome current)
        {
            var names = player.m_knownBiome;
            bool Matches(Biome biome) => names.Any(name => name.IndexOf(biome.ToString(), StringComparison.OrdinalIgnoreCase) >= 0);

            var recognized = BiomeCatalog.Order.Any(Matches);
            return BiomeCatalog.Order.Where(biome => biome == current || !recognized || Matches(biome)).ToList();
        }

        private void Build(InventoryGui gui)
        {
            font = GUIManager.Instance.TMP_AveriaSansLibre;
            if (gui.m_containerName is TMP_Text containerName && containerName.font != null) font = containerName.font;
            unlimitedText = font != null && !font.HasCharacter(Infinity[0], true, true) ? "MAX" : Infinity;

            if (!BuildButton(gui)) return;

            var parent = gui.m_trophiesPanel != null ? gui.m_trophiesPanel.transform.parent : gui.m_inventoryRoot;
            panel = GUIManager.Instance.CreateWoodpanel(parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                PanelWidth, PanelHeight, false);
            panel.name = PanelName;

            title = CreateText("Title", panel.transform, 28f, TextAlignmentOptions.Center, TitleColor);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(PanelWidth - 200f, 40f));

            summary = CreateText("Summary", panel.transform, 17f, TextAlignmentOptions.Center, Color.white);
            Place(summary.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -62f), new Vector2(PanelWidth - 60f, 26f));

            CreateButton("<", panel.transform, new Vector2(-(PanelWidth / 2f - 60f), -40f), 50f, () => Browse(-1));
            CreateButton(">", panel.transform, new Vector2(PanelWidth / 2f - 120f, -40f), 50f, () => Browse(1));
            CreateButton("X", panel.transform, new Vector2(PanelWidth / 2f - 55f, -40f), 40f, Close);

            var scroll = GUIManager.Instance.CreateScrollView(panel.transform, false, true, 10f, 5f,
                GUIManager.Instance.ValheimScrollbarHandleColorBlock, new Color(0f, 0f, 0f, 0.3f), ListWidth, PanelHeight - 130f);
            var scrollRect = (RectTransform)scroll.transform;
            scrollRect.anchorMin = scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRect.anchoredPosition = new Vector2(0f, -40f);

            content = scroll.GetComponentInChildren<ScrollRect>().content;
            if (!content.TryGetComponent(out VerticalLayoutGroup layout)) layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 2f;
            layout.padding = new RectOffset(4, 4, 4, 4);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            if (!content.TryGetComponent(out ContentSizeFitter fitter)) fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            emptyText = CreateText("Empty", panel.transform, 18f, TextAlignmentOptions.Center, SubtleColor);
            Place(emptyText.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(PanelWidth - 60f, 30f));
            emptyText.text = Texts.Get("bsc_gather_empty");

            panel.SetActive(false);
        }

        /// <summary>
        /// Adds the button as a copy of the trophies button, after the last of the game's own panel buttons.
        /// </summary>
        private bool BuildButton(InventoryGui gui)
        {
            var buttons = gui.GetComponentsInChildren<Button>(true);
            Button? Find(string method) => buttons.FirstOrDefault(button => Calls(button, method));

            var trophies = Find("OnOpenTrophies");
            var known = new[] { Find("OnOpenSkills"), Find("OnOpenTexts"), trophies, Find("OnOpenAchievements") }
                .Where(button => button != null).Select(button => button!).ToList();
            if (known.Count == 0)
            {
                Jotunn.Logger.LogWarning("Could not find the trophies button in the inventory screen; the gathering panel is not available.");
                return false;
            }

            var template = trophies ?? known[0];
            var copy = UiCopy.Create(template.gameObject, ButtonName);
            var copyButton = copy.GetComponent<Button>();
            copyButton.onClick = new Button.ButtonClickedEvent();
            copyButton.onClick.AddListener(Toggle);

            SetIcon(copy, copyButton, buttonIcon());
            if (copy.TryGetComponent(out UITooltip tooltip))
            {
                tooltip.m_topic = string.Empty;
                tooltip.m_text = Texts.Get("bsc_gather_button");
            }

            var parent = template.transform.parent;
            if (parent.GetComponent<LayoutGroup>() != null)
            {
                copy.transform.SetAsLastSibling();
                return true;
            }

            var siblings = known.Where(button => button.transform.parent == parent)
                .Select(button => ((RectTransform)button.transform).anchoredPosition).ToList();
            var horizontal = siblings.Max(p => p.x) - siblings.Min(p => p.x) >= siblings.Max(p => p.y) - siblings.Min(p => p.y);
            siblings = horizontal ? siblings.OrderBy(p => p.x).ToList() : siblings.OrderByDescending(p => p.y).ToList();

            var templateRect = (RectTransform)template.transform;
            var step = siblings.Count >= 2
                ? (siblings[siblings.Count - 1] - siblings[0]) / (siblings.Count - 1)
                : new Vector2(templateRect.rect.width + 4f, 0f);
            ((RectTransform)copy.transform).anchoredPosition = siblings[siblings.Count - 1] + step;
            return true;
        }

        private static bool Calls(Button button, string method)
        {
            for (var i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                if (button.onClick.GetPersistentMethodName(i) == method) return true;
            }
            return false;
        }

        // The icon is drawn on top of the button's frame, so it is the last image with a sprite.
        private static void SetIcon(GameObject copy, Button button, Sprite? icon)
        {
            if (icon == null) return;

            var image = copy.GetComponentsInChildren<Image>(true)
                .LastOrDefault(candidate => candidate.gameObject != copy && candidate != button.targetGraphic && candidate.sprite != null);
            if (image == null) image = button.targetGraphic as Image;
            if (image != null) image.sprite = icon;
        }

        private Row CreateRow(Transform parent, int index)
        {
            var root = new GameObject("Row", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            var element = root.AddComponent<LayoutElement>();
            element.minHeight = element.preferredHeight = RowHeight;
            var background = root.AddComponent<Image>();
            background.color = new Color(1f, 1f, 1f, index % 2 == 0 ? 0.06f : 0.02f);
            background.raycastTarget = false;
            var group = root.AddComponent<CanvasGroup>();

            var iconObject = new GameObject("Icon", typeof(RectTransform));
            iconObject.transform.SetParent(root.transform, false);
            var icon = iconObject.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            Place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(6f, 0f), new Vector2(36f, 36f));

            var name = CreateText("Name", root.transform, 17f, TextAlignmentOptions.Left, Color.white);
            Place(name.rectTransform, new Vector2(0f, 0.5f), new Vector2(50f, 9f), new Vector2(230f, 22f));

            var chest = CreateText("Chest", root.transform, 13f, TextAlignmentOptions.Left, SubtleColor);
            Place(chest.rectTransform, new Vector2(0f, 0.5f), new Vector2(50f, -11f), new Vector2(230f, 18f));

            var bar = new GameObject("Bar", typeof(RectTransform));
            bar.transform.SetParent(root.transform, false);
            var barImage = bar.AddComponent<Image>();
            barImage.color = BarBackgroundColor;
            barImage.raycastTarget = false;
            Place((RectTransform)bar.transform, new Vector2(0f, 0.5f), new Vector2(290f, 0f), new Vector2(150f, 18f));

            var fillObject = new GameObject("Fill", typeof(RectTransform));
            fillObject.transform.SetParent(bar.transform, false);
            var fillImage = fillObject.AddComponent<Image>();
            fillImage.color = BarColor;
            fillImage.raycastTarget = false;
            var fill = fillImage.rectTransform;
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0f, 1f);
            fill.offsetMin = fill.offsetMax = Vector2.zero;

            var amount = CreateText("Amount", bar.transform, 14f, TextAlignmentOptions.Center, Color.white);
            amount.rectTransform.anchorMin = Vector2.zero;
            amount.rectTransform.anchorMax = Vector2.one;
            amount.rectTransform.offsetMin = amount.rectTransform.offsetMax = Vector2.zero;

            var unlimited = CreateText("Unlimited", root.transform, 28f, TextAlignmentOptions.Center, Color.white);
            Place(unlimited.rectTransform, new Vector2(0f, 0.5f), new Vector2(290f, 0f), new Vector2(150f, 36f));

            var status = CreateText("Status", root.transform, 14f, TextAlignmentOptions.Left, Color.white);
            Place(status.rectTransform, new Vector2(0f, 0.5f), new Vector2(450f, 9f), new Vector2(ListWidth - 465f, 20f));

            var bag = CreateText("Bag", root.transform, 13f, TextAlignmentOptions.Left, BagColor);
            Place(bag.rectTransform, new Vector2(0f, 0.5f), new Vector2(450f, -11f), new Vector2(ListWidth - 465f, 18f));

            return new Row(root, group, icon, name, chest, bar, fill, amount, unlimited, status, bag);
        }

        private TMP_Text CreateText(string name, Transform parent, float size, TextAlignmentOptions alignment, Color color)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            // Inactive until the font is set; TextMeshPro otherwise looks for a default font the game does not ship.
            textObject.SetActive(false);
            textObject.transform.SetParent(parent, false);

            var text = textObject.AddComponent<TextMeshProUGUI>();
            if (font != null) text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = color;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false;
            textObject.SetActive(true);
            return text;
        }

        private static void CreateButton(string label, Transform parent, Vector2 position, float width, Action onClick)
        {
            var buttonObject = GUIManager.Instance.CreateButton(label, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                position, width, 40f);
            buttonObject.GetComponent<Button>().onClick.AddListener(() => onClick());
        }

        // Anchors a rectangle to one point of its parent, with the pivot on the same point.
        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private enum EntryState
        {
            Progress,
            NotDiscovered,
            Unlimited,
            Normal
        }

        private sealed class Entry
        {
            public Entry(ItemDrop.ItemData.SharedData shared, string displayName, string? chestName)
            {
                Shared = shared;
                DisplayName = displayName;
                ChestName = chestName;
            }

            public ItemDrop.ItemData.SharedData Shared { get; }

            public string DisplayName { get; }

            public string? ChestName { get; }

            public EntryState State;

            public int Stored;

            public int Required;

            public int InBag;
        }

        private sealed class Row
        {
            public Row(GameObject root, CanvasGroup group, Image icon, TMP_Text name, TMP_Text chest, GameObject bar,
                RectTransform fill, TMP_Text amount, TMP_Text unlimited, TMP_Text status, TMP_Text bag)
            {
                Root = root;
                Group = group;
                Icon = icon;
                Name = name;
                Chest = chest;
                Bar = bar;
                Fill = fill;
                Amount = amount;
                Unlimited = unlimited;
                Status = status;
                Bag = bag;
            }

            public GameObject Root { get; }

            public CanvasGroup Group { get; }

            public Image Icon { get; }

            public TMP_Text Name { get; }

            public TMP_Text Chest { get; }

            public GameObject Bar { get; }

            public RectTransform Fill { get; }

            public TMP_Text Amount { get; }

            public TMP_Text Unlimited { get; }

            public TMP_Text Status { get; }

            public TMP_Text Bag { get; }
        }
    }
}
