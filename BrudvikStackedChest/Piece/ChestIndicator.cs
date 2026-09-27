using BrudvikStackedChest.Constants;
using BrudvikStackedChest.Helpers;
using UnityEngine;

namespace BrudvikStackedChest.Piece
{
    /// <summary>
    /// Shows on the front of a chest whether it is empty, how many of its slots are used and how many of its items
    /// are unlimited. Every client computes this from its own copy of the chest contents.
    /// </summary>
    public class ChestIndicator : MonoBehaviour
    {
        private const string IconName = "Icon";
        private const float BarHeight = 0.022f;
        private const float BarGap = 0.008f;

        private static readonly Color EmptyIconColor = new(0.3f, 0.3f, 0.3f, 1f);
        private static readonly Color BackgroundColor = new(0.05f, 0.05f, 0.05f, 0.85f);
        private static readonly Color SlotsColor = new(0.85f, 0.85f, 0.85f, 1f);
        private static readonly Color UnlimitedColor = new(1f, 0.82f, 0.3f, 1f);

        private static Sprite? pixel;

        private SpriteRenderer? icon;
        private GameObject? slotsBar;
        private GameObject? unlimitedBar;
        private Transform? slotsFill;
        private Transform? unlimitedFill;

        /// <summary>
        /// Updates the icon and the bars from the chest contents.
        /// </summary>
        /// <param name="inventory">The chest's inventory.</param>
        /// <param name="category">The chest's item category.</param>
        /// <param name="supply">Decides which items are unlimited.</param>
        /// <param name="visible">Whether the indicators are switched on.</param>
        public void Refresh(Inventory inventory, ChestCategory category, ChestSupply supply, bool visible)
        {
            if (slotsBar == null) Build();

            var used = inventory.NrOfItems();
            var slots = Mathf.Max(1, inventory.GetWidth() * inventory.GetHeight());

            var supplied = 0;
            var total = 0;
            if (supply.Mode != ChestMode.Full && category != ChestCategory.None) supply.CountProgress(category, out supplied, out total);

            if (icon != null) icon.color = visible && used == 0 ? EmptyIconColor : Color.white;

            slotsBar!.SetActive(visible);
            SetFill(slotsFill!, (float)used / slots);

            unlimitedBar!.SetActive(visible && total > 0);
            if (total > 0) SetFill(unlimitedFill!, (float)supplied / total);
        }

        private void Build()
        {
            var iconTransform = transform.Find(IconName);
            icon = iconTransform == null ? null : iconTransform.GetComponent<SpriteRenderer>();

            // Place the bars right below the icon and as wide as it.
            var center = iconTransform == null ? new Vector3(0.2f, 0.5f, 0.48f) : iconTransform.localPosition;
            var size = icon != null && icon.sprite != null ? icon.sprite.bounds.size.x * iconTransform!.localScale.x : 0.19f;
            var top = center.y - size / 2f - BarGap;

            slotsBar = CreateBar("SlotsBar", new Vector3(center.x - size / 2f, top - BarHeight / 2f, center.z), size, SlotsColor, out slotsFill);
            unlimitedBar = CreateBar("UnlimitedBar", new Vector3(center.x - size / 2f, top - BarGap - BarHeight * 1.5f, center.z), size, UnlimitedColor, out unlimitedFill);
        }

        private GameObject CreateBar(string name, Vector3 leftCenter, float width, Color color, out Transform fill)
        {
            var bar = new GameObject(name);
            bar.transform.SetParent(transform, false);
            bar.transform.localPosition = leftCenter;

            CreateQuad("Background", bar.transform, width, BackgroundColor, 1);
            fill = CreateQuad("Fill", bar.transform, width, color, 2).transform;
            return bar;
        }

        private static GameObject CreateQuad(string name, Transform parent, float width, Color color, int sortingOrder)
        {
            var quad = new GameObject(name);
            quad.transform.SetParent(parent, false);
            quad.transform.localScale = new Vector3(width, BarHeight, 1f);

            var renderer = quad.AddComponent<SpriteRenderer>();
            renderer.sprite = GetPixel();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return quad;
        }

        private static void SetFill(Transform fill, float fraction)
        {
            var scale = fill.localScale;
            fill.localScale = new Vector3(fill.parent.GetChild(0).localScale.x * Mathf.Clamp01(fraction), scale.y, scale.z);
        }

        // One unit wide with the pivot on the left edge, so scaling it in x fills a bar from the left.
        private static Sprite GetPixel()
        {
            if (pixel == null)
            {
                var texture = Texture2D.whiteTexture;
                pixel = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0f, 0.5f), texture.width);
            }
            return pixel;
        }
    }
}
