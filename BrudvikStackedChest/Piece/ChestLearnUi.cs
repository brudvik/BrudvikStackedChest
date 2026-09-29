using BrudvikStackedChest.Events;
using BrudvikStackedChest.Helpers;
using BrudvikStackedChest.Utils;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BrudvikStackedChest.Piece
{
    /// <summary>
    /// Marks the items in an open chest that the player has not learned yet, and adds a Learn all button that
    /// teaches the player all of them at once.
    /// </summary>
    public class ChestLearnUi
    {
        private const string MarkerName = "BrudvikStackedChest_Unknown";
        private const string ButtonName = "BrudvikStackedChest_LearnAll";
        private const string Gold = "#FFD24D";
        private static readonly Color MarkerColor = new(1f, 0.85f, 0.1f, 1f);

        private readonly Func<Container?, CustomPieceExtended?> findPiece;
        private readonly Func<bool> learnAllEnabled;
        private readonly Func<bool> learnTrophies;
        private readonly List<GameObject> markers = new();

        private InventoryGui? builtFor;
        private Button? button;
        private TMP_Text? buttonText;
        private bool learning;
        private int learnedRecipes;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChestLearnUi"/> class.
        /// </summary>
        /// <param name="findPiece">Finds the chest definition for a container, or null if it is not one of ours.</param>
        /// <param name="learnAllEnabled">Tells whether the Learn all button is switched on.</param>
        /// <param name="learnTrophies">Tells whether the Learn all button learns trophies as well.</param>
        public ChestLearnUi(Func<Container?, CustomPieceExtended?> findPiece, Func<bool> learnAllEnabled, Func<bool> learnTrophies)
        {
            this.findPiece = findPiece;
            this.learnAllEnabled = learnAllEnabled;
            this.learnTrophies = learnTrophies;
        }

        /// <summary>
        /// Raised with the items' localization tokens after the player has learned the items in a chest.
        /// </summary>
        public event Action<List<string>>? ItemsLearned;

        /// <summary>
        /// Shows a yellow exclamation mark on the items in the open chest that the player has not learned yet.
        /// </summary>
        public void HandleGridUpdated(object sender, InventoryGridUpdatedPatchEvent e)
        {
            var gui = InventoryGui.instance;
            if (gui == null || e.Grid != gui.m_containerGrid) return;

            markers.RemoveAll(marker => marker == null);
            foreach (var marker in markers)
            {
                marker.SetActive(false);
            }

            var player = Player.m_localPlayer;
            if (player == null || findPiece(gui.m_currentContainer) == null) return;

            var inventory = e.Grid.m_inventory;
            foreach (var item in inventory.GetAllItems())
            {
                if (player.m_knownMaterial.Contains(item.m_shared.m_name)) continue;

                var element = e.Grid.GetElement(item.m_gridPos.x, item.m_gridPos.y, e.Grid.m_width);
                if (element != null) GetMarker(element).SetActive(true);
            }
        }

        /// <summary>
        /// Tells the player in the item tooltip that they have not learned the item yet.
        /// </summary>
        public void HandleItemTooltip(object sender, ItemTooltipPatchEvent e)
        {
            var gui = InventoryGui.instance;
            var player = Player.m_localPlayer;
            if (gui == null || player == null || e.Grid != gui.m_containerGrid || findPiece(gui.m_currentContainer) == null) return;
            if (player.m_knownMaterial.Contains(e.Item.m_shared.m_name)) return;

            e.Tooltip.Set(e.Tooltip.m_topic, $"{e.Tooltip.m_text}\n<color={Gold}>{Texts.Get("bsc_status_not_learned")}</color>",
                e.Grid.m_tooltipAnchor, Vector2.zero);
        }

        /// <summary>
        /// Shows the Learn all button when the open chest holds items the player can learn.
        /// </summary>
        public void HandleContainerPanelUpdated(object sender, ContainerPanelUpdatedPatchEvent e)
        {
            if (builtFor != e.Gui && !TryBuild(e.Gui)) return;

            var player = Player.m_localPlayer;
            var container = e.Gui.m_currentContainer;
            var inventory = container == null ? null : container.GetInventory();
            var count = player == null || inventory == null || !learnAllEnabled() || findPiece(container) == null
                ? 0
                : CollectLearnable(player, inventory).Count;

            button!.gameObject.SetActive(count > 0);
            if (count > 0 && buttonText != null) buttonText.text = Texts.Get("bsc_learn_button", count);
        }

        /// <summary>
        /// Leaves out the game's messages for every single item and recipe while the player learns a chest.
        /// </summary>
        public void HandleUnlockMessage(object sender, UnlockMessagePatchEvent e)
        {
            if (!learning) return;

            e.Skip = true;
            if (e.Topic == "$msg_newrecipe" || e.Topic == "$msg_newpiece" || e.Topic == "$msg_newdish") learnedRecipes++;
        }

        private void LearnOpenChest()
        {
            var gui = InventoryGui.instance;
            var player = Player.m_localPlayer;
            var container = gui == null ? null : gui.m_currentContainer;
            var inventory = container == null ? null : container.GetInventory();
            if (player == null || inventory == null || !learnAllEnabled() || findPiece(container) == null) return;

            var items = CollectLearnable(player, inventory);
            if (items.Count == 0) return;

            var learned = new List<string>();
            learning = true;
            learnedRecipes = 0;
            try
            {
                foreach (var item in items)
                {
                    if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophy) player.AddTrophy(item);
                    if (player.m_knownMaterial.Add(item.m_shared.m_name)) learned.Add(item.m_shared.m_name);
                }

                player.UpdateKnownRecipesList();
                player.UpdateEvents();
            }
            finally
            {
                learning = false;
            }

            player.Message(MessageHud.MessageType.Center, Texts.Get("bsc_learn_done", learned.Count, learnedRecipes));
            ItemsLearned?.Invoke(learned);
        }

        private List<ItemDrop.ItemData> CollectLearnable(Player player, Inventory inventory)
        {
            var result = new List<ItemDrop.ItemData>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var trophies = learnTrophies();
            foreach (var item in inventory.GetAllItems())
            {
                var token = item.m_shared.m_name;
                if (player.m_knownMaterial.Contains(token) || !seen.Add(token)) continue;
                if (!trophies && item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophy) continue;

                result.Add(item);
            }
            return result;
        }

        private GameObject GetMarker(InventoryElement element)
        {
            var existing = element.transform.Find(MarkerName);
            if (existing != null) return existing.gameObject;

            var marker = new GameObject(MarkerName, typeof(RectTransform));
            // Inactive until the font is set; TextMeshPro otherwise looks for a default font the game does not ship.
            marker.SetActive(false);
            marker.transform.SetParent(element.transform, false);

            var rect = (RectTransform)marker.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(4f, -1f);
            rect.sizeDelta = new Vector2(16f, 24f);

            var text = marker.AddComponent<TextMeshProUGUI>();
            if (element.m_amount != null) text.font = element.m_amount.font;
            text.text = "!";
            text.fontSize = 22f;
            text.fontStyle = FontStyles.Bold;
            text.color = MarkerColor;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.raycastTarget = false;

            markers.Add(marker);
            return marker;
        }

        /// <summary>
        /// Creates the Learn all button as a copy of the Take all button, placed after the Place stacks button.
        /// </summary>
        private bool TryBuild(InventoryGui gui)
        {
            var takeAll = gui.m_takeAllButton;
            var stackAll = gui.m_stackAllButton;
            if (takeAll == null) return false;

            builtFor = gui;
            var existing = takeAll.transform.parent.Find(ButtonName);
            var copy = existing != null ? existing.gameObject : UiCopy.Create(takeAll.gameObject, ButtonName);

            button = copy.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(LearnOpenChest);
            buttonText = copy.GetComponentInChildren<TMP_Text>(true);

            var takeRect = (RectTransform)takeAll.transform;
            var copyRect = (RectTransform)copy.transform;
            if (stackAll != null && existing == null)
            {
                var stackRect = (RectTransform)stackAll.transform;
                var step = stackRect.anchoredPosition - takeRect.anchoredPosition;
                if (step == Vector2.zero) step = new Vector2(0f, -(takeRect.rect.height + 4f));
                copyRect.anchoredPosition = stackRect.anchoredPosition + step;
            }

            copy.SetActive(false);
            return true;
        }
    }

    /// <summary>
    /// Copies buttons of the game's own screens, without the parts that tie them to the game's behavior.
    /// </summary>
    internal static class UiCopy
    {
        /// <summary>
        /// Copies a UI object next to the original. Gamepad shortcuts and automatic translations are removed from
        /// the copy, so it neither reacts to the original's gamepad button nor gets its text replaced.
        /// </summary>
        /// <param name="original">The object to copy.</param>
        /// <param name="name">The name of the copy.</param>
        /// <returns>The copy.</returns>
        public static GameObject Create(GameObject original, string name)
        {
            var copy = Object.Instantiate(original, original.transform.parent);
            copy.name = name;

            foreach (var gamePad in copy.GetComponentsInChildren<UIGamePad>(true))
            {
                if (gamePad.m_hint != null && gamePad.m_hint.transform.IsChildOf(copy.transform)) Object.DestroyImmediate(gamePad.m_hint);
                Object.DestroyImmediate(gamePad);
            }

            foreach (var localize in copy.GetComponentsInChildren<Localize>(true))
            {
                Object.DestroyImmediate(localize);
            }

            return copy;
        }
    }
}
