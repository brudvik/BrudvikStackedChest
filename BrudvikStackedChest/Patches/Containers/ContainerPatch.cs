using BrudvikStackedChest.Events;
using HarmonyLib;
using System;

namespace BrudvikStackedChest.Patches.Containers
{

    /// <summary>
    /// This class contains patches for the Container class to hook into its methods.
    /// It raises events when certain methods of the Container class are called.
    /// </summary>
    public class ContainerPatch 
    {
        /// <summary>
        /// Event triggered when the Awake method of a Container instance is called.
        /// </summary>
        public static event EventHandler<ContainerAwakePatchEvent>? ContainerAwakePatched;

        /// <summary>
        /// Event triggered when the CheckForChanges method of a Container instance is called.
        /// </summary>
        public static event EventHandler<ContainerCheckForChangesPatchEvent>? ContainerCheckForChangesPatched;

        public static event EventHandler<ContainerDropAllItemsPatchEvent>? ContainerDropAllItemsPatched;

        /// <summary>
        /// Harmony patch for the Awake method of the Container class.
        /// This patch triggers the ContainerAwakePatched event after the original Awake method is executed.
        /// </summary>
        [HarmonyPatch(typeof(Container), "Awake")]
        public static class ContainerAwakePatch
        {
            /// <summary>
            /// Postfix method that is called after the original Awake method of the Container class.
            /// </summary>
            /// <param name="__instance">The instance of the Container class.</param>
            static void Postfix(Container __instance)
            {
                if (__instance != null)
                {
                    ContainerAwakePatched?.Invoke(null, new ContainerAwakePatchEvent()
                    {
                        Container = __instance
                    });
                }
            }
        }

        /// <summary>
        /// Harmony patch for the CheckForChanges method of the Container class.
        /// This patch triggers the ContainerCheckForChangesPatched event after the original CheckForChanges method is executed.
        /// </summary>
        [HarmonyPatch(typeof(Container), "CheckForChanges")]
        public static class ContainerCheckForChangesPatch
        {
            /// <summary>
            /// Postfix method that is called after the original CheckForChanges method of the Container class.
            /// </summary>
            /// <param name="__instance">The instance of the Container class.</param>
            static void Postfix(Container __instance)
            {
                if (__instance != null)
                {
                    ContainerCheckForChangesPatched?.Invoke(null, new ContainerCheckForChangesPatchEvent()
                    {
                        Container = __instance
                    });
                }
            }
        }

        /// <summary>
        /// This Harmony patch targets the 'DropAllItems' method of the 'Container' class.
        /// The 'new Type[] { }' specifies that this patch is for the parameterless 'DropAllItems' method.
        /// </summary>
        [HarmonyPatch(typeof(Container), "DropAllItems")]
        [HarmonyPatch(new Type[] { })]
        public static class ContainerDropAllItemsPatch
        {
            // The Prefix method is executed before the original 'DropAllItems' method.
            static void Prefix(Container __instance)
            {
                // Check if the instance of the Container class is not null.
                if (__instance != null)
                {
                    // Trigger the 'ContainerDropAllItemsPatched' event.
                    // This event can be used to perform additional actions or logging before the original method is executed.
                    ContainerDropAllItemsPatched?.Invoke(null, new ContainerDropAllItemsPatchEvent()
                    {
                        Container = __instance // Pass the instance of the Container class to the event.
                    });
                }
            }
        }

    }

}
