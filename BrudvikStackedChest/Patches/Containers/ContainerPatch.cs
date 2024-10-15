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

    }

}
