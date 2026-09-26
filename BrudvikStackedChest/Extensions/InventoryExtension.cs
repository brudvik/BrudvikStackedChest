using System.Collections.Generic;

namespace BrudvikStackedChest.Extensions
{
    /// <summary>
    /// Extension methods for the Inventory class.
    /// </summary>
    public static class InventoryExtension
    {
        /// <summary>
        /// Adds up the stacks of each item in the inventory.
        /// </summary>
        /// <param name="inventory">The inventory to count.</param>
        /// <returns>The total amount per item prefab name.</returns>
        public static Dictionary<string, int> CountByPrefab(this Inventory inventory)
        {
            var totals = new Dictionary<string, int>();
            foreach (var item in inventory.GetAllItems())
            {
                if (item.m_dropPrefab == null) continue;

                totals.TryGetValue(item.m_dropPrefab.name, out var total);
                totals[item.m_dropPrefab.name] = total + item.m_stack;
            }
            return totals;
        }
    }
}
