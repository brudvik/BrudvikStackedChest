using System.Collections.Generic;

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
            // Load the container's inventory
            container.Load();
            var inventory = container.GetInventory();
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
            var inventory = container.GetInventory();
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
            return container.GetInventory().GetAllItems().Count;
        }
    }
}
