using System.Collections.Generic;
using UnityEngine;

namespace BrudvikStackedChest.Extensions
{
    /// <summary>
    /// Extension methods for the Container class to handle item spawning and refilling.
    /// </summary>
    public static class ContainerExtension
    {
        /// <summary>
        /// Spawns initial items in the container if they do not already exist.
        /// Also refills existing items to their maximum stack size.
        /// </summary>
        /// <param name="container">The container in which to spawn items.</param>
        /// <param name="spawnItems">A list of item names to spawn in the container.</param>
        public static void SpawnInitialItems(this Container container, List<string> spawnItems)
        {
            // Check if the container is placed in the game world. We need to check
            // this before attempting to load the container's inventory. If the container
            // is not placed, we cannot load the inventory.
            if (!IsContainerPlaced(container))
            {
                return;
            }

            // Load the container's inventory
            container.Load();

            // Retrieve the inventory of the container.
            var inventory = container.GetInventory();
            if (inventory == null) return;

            var existingItems = new HashSet<string>();

            // Collect existing items in the inventory and refill them to their maximum stack size
            foreach (var item in inventory.GetAllItems())
            {
                existingItems.Add(item.m_dropPrefab.name);
                item.m_stack = item.m_shared.m_maxStackSize;
            }

            // Add new items to the inventory if they do not already exist
            foreach (var prefabName in spawnItems)
            {
                if (!existingItems.Contains(prefabName))
                {
                    var prefab = ObjectDB.instance.GetItemPrefab(prefabName);
                    if (prefab != null)
                    {
                        var itemDrop = prefab.GetComponent<ItemDrop>();
                        if (itemDrop != null)
                        {
                            inventory.AddItem(itemDrop.m_itemData.m_dropPrefab, itemDrop.m_itemData.m_shared.m_maxStackSize);
                        }
                    }
                }
            }

            // Save the container's state
            container.Save();
        }

        /// <summary>
        /// Refills all items in the container to their maximum stack size.
        /// </summary>
        /// <param name="container">The container whose items should be refilled.</param>
        public static void RefillItemsToMax(this Container container)
        {
            // Retrieve the inventory of the container.
            var inventory = container.GetInventory();
            if (inventory == null) return;

            // Refill each item in the inventory to its maximum stack size
            foreach (var item in inventory.GetAllItems())
            {
                item.m_stack = item.m_shared.m_maxStackSize;
            }

            // Save the container's state
            container.Save();
        }

        /// <summary>
        /// Gets the count of items in the container's inventory.
        /// </summary>
        /// <param name="container">The container whose inventory count is to be retrieved.</param>
        /// <returns>The number of items in the container's inventory.</returns>
        public static int GetInventoryCount(this Container container)
        {
            // Retrieve the inventory of the container.
            var inventory = container.GetInventory();
            if (inventory == null) return 0;
            return inventory.GetAllItems().Count;
        }

        /// <summary>
        /// Empties all items from the container's inventory.
        /// </summary>
        /// <param name="container">The container to be emptied.</param>
        public static void EmptyChest(this Container container)
        {
            // Retrieve the inventory of the container.
            var inventory = container.GetInventory();
            if (inventory == null) return;

            // Remove all items from the inventory.
            inventory.RemoveAll();

            // Save the state of the container to persist the changes.
            container.Save();
        }

        /// <summary>
        /// Check if the container is actually placed in the game world.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        private static bool IsContainerPlaced(Container container)
        {
            // Check if the container has a valid position in the game world
            return container.transform != null && container.transform.position != Vector3.zero;
        }
    }
}
