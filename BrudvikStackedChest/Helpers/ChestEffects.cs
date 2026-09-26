using BrudvikStackedChest.Constants;
using Jotunn.Managers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BrudvikStackedChest.Helpers
{
    /// <summary>
    /// Plays the game's own visual effects on a chest.
    /// </summary>
    public static class ChestEffects
    {
        /// <summary>Sparkle shown when a chest refills its stacks.</summary>
        public const string Refill = "vfx_pick_wisp";

        /// <summary>Shown when an item is unlocked in Linear mode.</summary>
        public const string Unlock = "fx_HildirChest_Unlock";

        /// <summary>Shown when every item of a chest has become unlimited.</summary>
        public const string Complete = "vfx_HealthUpgrade";

        /// <summary>The glow intensity of a chest that shows no progress.</summary>
        public const float BaseGlowIntensity = 0.5f;

        private static readonly Color CompletedGlowColor = new(1f, 0.82f, 0.3f, 1f);
        private static readonly Dictionary<int, bool> lastCompleted = new();

        // Chests refill every second while items are taken, e.g. when building from chests.
        private const float RefillCooldownSeconds = 10f;

        private static readonly Dictionary<string, EffectList?> effects = new();
        private static readonly Dictionary<int, float> lastRefillEffect = new();

        /// <summary>
        /// Plays the refill sparkle on the chest, at most once per cooldown period for each chest.
        /// </summary>
        /// <param name="container">The chest.</param>
        public static void PlayRefill(Container container)
        {
            var id = container.GetInstanceID();
            var now = Time.time;
            if (lastRefillEffect.TryGetValue(id, out var last) && now - last < RefillCooldownSeconds) return;

            if (lastRefillEffect.Count > 256) ForgetExpiredRefills(now);
            lastRefillEffect[id] = now;
            Play(container, Refill);
        }

        /// <summary>
        /// Makes the chest glow brighter the more of its items are unlimited, turns it gold when every item is, and
        /// plays a celebration effect when that happens. Runs on every client, so each player sees the same glow.
        /// </summary>
        /// <param name="container">The chest.</param>
        /// <param name="tint">The chest's color.</param>
        /// <param name="category">The chest's item category.</param>
        /// <param name="supply">Decides which items are unlimited.</param>
        public static void UpdateGlow(Container container, Color tint, ChestCategory category, ChestSupply supply)
        {
            var light = container.GetComponentInChildren<Light>();
            if (light == null) return;

            var supplied = 0;
            var total = 0;
            if (supply.Mode != ChestMode.Full && category != ChestCategory.None) supply.CountProgress(category, out supplied, out total);

            var completed = total > 0 && supplied == total;
            light.color = completed ? CompletedGlowColor : GetGlowColor(tint);
            light.intensity = total == 0 ? BaseGlowIntensity : completed ? 1.2f : Mathf.Lerp(0.2f, 1f, (float)supplied / total);

            // Until the progress is known, a chest would look incomplete and then suddenly complete.
            if (!supply.IsReady) return;

            var id = container.GetInstanceID();
            if (lastCompleted.TryGetValue(id, out var wasCompleted) && !wasCompleted && completed &&
                container.m_nview != null && container.m_nview.IsValid() && container.m_nview.IsOwner())
            {
                Play(container, Complete);
            }

            if (lastCompleted.Count > 512) lastCompleted.Clear();
            lastCompleted[id] = completed;
        }

        /// <summary>
        /// Gets the light color for a chest tint. A dark tint gives almost no light, so the tint's hue is used at
        /// full brightness, and a black tint gets a warm white light.
        /// </summary>
        /// <param name="tint">The chest's color.</param>
        /// <returns>The light color.</returns>
        public static Color GetGlowColor(Color tint)
        {
            var brightest = Mathf.Max(tint.r, tint.g, tint.b);
            if (brightest < 0.05f) return new Color(1f, 0.9f, 0.7f, 1f);

            return new Color(tint.r / brightest, tint.g / brightest, tint.b / brightest, 1f);
        }

        private static void ForgetExpiredRefills(float now)
        {
            foreach (var id in lastRefillEffect.Where(pair => now - pair.Value >= RefillCooldownSeconds).Select(pair => pair.Key).ToList())
            {
                lastRefillEffect.Remove(id);
            }
        }

        /// <summary>
        /// Plays an effect just above the chest.
        /// </summary>
        /// <param name="container">The chest.</param>
        /// <param name="prefabName">The effect prefab name.</param>
        public static void Play(Container container, string prefabName)
        {
            var effect = GetEffect(prefabName);
            effect?.Create(container.transform.position + Vector3.up * 0.8f, Quaternion.identity);
        }

        private static EffectList? GetEffect(string prefabName)
        {
            if (effects.TryGetValue(prefabName, out var cached)) return cached;

            var prefab = PrefabManager.Instance.GetPrefab(prefabName);
            if (prefab == null)
            {
                // Game data is not loaded yet; try again next time.
                if (ZNetScene.instance == null) return null;

                Jotunn.Logger.LogWarning($"Effect prefab '{prefabName}' was not found; the effect is disabled.");
            }

            var effect = prefab == null
                ? null
                : new EffectList { m_effectPrefabs = new[] { new EffectList.EffectData { m_prefab = prefab, m_enabled = true } } };
            effects[prefabName] = effect;
            return effect;
        }
    }
}
