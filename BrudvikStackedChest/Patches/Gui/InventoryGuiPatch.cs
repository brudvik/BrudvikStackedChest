using BrudvikStackedChest.Events;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BrudvikStackedChest.Patches.Gui
{
    /// <summary>
    /// Patches for the inventory screen that raise events when the open container is drawn.
    /// </summary>
    public class InventoryGuiPatch
    {
        /// <summary>
        /// Event triggered after an inventory grid has updated its slots.
        /// </summary>
        public static event EventHandler<InventoryGridUpdatedPatchEvent>? InventoryGridUpdatedPatched;

        /// <summary>
        /// Event triggered after an inventory grid has built an item tooltip.
        /// </summary>
        public static event EventHandler<ItemTooltipPatchEvent>? ItemTooltipPatched;

        /// <summary>
        /// Event triggered after the open container panel has been updated.
        /// </summary>
        public static event EventHandler<ContainerPanelUpdatedPatchEvent>? ContainerPanelUpdatedPatched;

        /// <summary>
        /// Harmony patch for InventoryGrid.UpdateGui.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGrid), "UpdateGui")]
        public static class InventoryGridUpdateGuiPatch
        {
            static void Postfix(InventoryGrid __instance)
            {
                if (__instance != null)
                {
                    InventoryGridUpdatedPatched?.Invoke(null, new InventoryGridUpdatedPatchEvent { Grid = __instance });
                }
            }
        }

        /// <summary>
        /// Harmony patch for InventoryGrid.CreateItemTooltip.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGrid), "CreateItemTooltip")]
        public static class InventoryGridCreateItemTooltipPatch
        {
            static void Postfix(InventoryGrid __instance, ItemDrop.ItemData item, UITooltip tooltip)
            {
                if (__instance != null && item != null && tooltip != null)
                {
                    ItemTooltipPatched?.Invoke(null, new ItemTooltipPatchEvent { Grid = __instance, Item = item, Tooltip = tooltip });
                }
            }
        }

        /// <summary>
        /// Event triggered after the inventory screen has been shown.
        /// </summary>
        public static event EventHandler<InventoryGuiPatchEvent>? InventoryShownPatched;

        /// <summary>
        /// Event triggered after the inventory screen has been hidden.
        /// </summary>
        public static event EventHandler<InventoryGuiPatchEvent>? InventoryHiddenPatched;

        /// <summary>
        /// Event triggered after the inventory screen has opened its skills, texts, trophies or achievements panel.
        /// </summary>
        public static event EventHandler<InventoryGuiPatchEvent>? InventoryPanelOpenedPatched;

        /// <summary>
        /// Harmony patch for InventoryGui.UpdateContainer.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGui), "UpdateContainer")]
        public static class InventoryGuiUpdateContainerPatch
        {
            static void Postfix(InventoryGui __instance)
            {
                if (__instance != null)
                {
                    ContainerPanelUpdatedPatched?.Invoke(null, new ContainerPanelUpdatedPatchEvent { Gui = __instance });
                }
            }
        }

        /// <summary>
        /// Harmony patch for InventoryGui.Show.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGui), "Show")]
        public static class InventoryGuiShowPatch
        {
            static void Postfix(InventoryGui __instance)
            {
                if (__instance != null)
                {
                    InventoryShownPatched?.Invoke(null, new InventoryGuiPatchEvent { Gui = __instance });
                }
            }
        }

        /// <summary>
        /// Harmony patch for InventoryGui.Hide.
        /// </summary>
        [HarmonyPatch(typeof(InventoryGui), "Hide")]
        public static class InventoryGuiHidePatch
        {
            static void Postfix(InventoryGui __instance)
            {
                if (__instance != null)
                {
                    InventoryHiddenPatched?.Invoke(null, new InventoryGuiPatchEvent { Gui = __instance });
                }
            }
        }

        /// <summary>
        /// Harmony patch for the methods that open the inventory screen's own panels.
        /// </summary>
        [HarmonyPatch]
        public static class InventoryGuiOpenPanelPatch
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(InventoryGui), "OnOpenSkills");
                yield return AccessTools.Method(typeof(InventoryGui), "OnOpenTexts");
                yield return AccessTools.Method(typeof(InventoryGui), "OnOpenTrophies");
                yield return AccessTools.Method(typeof(InventoryGui), "OnOpenAchievements");
            }

            static void Postfix(InventoryGui __instance)
            {
                if (__instance != null)
                {
                    InventoryPanelOpenedPatched?.Invoke(null, new InventoryGuiPatchEvent { Gui = __instance });
                }
            }
        }
    }
}
