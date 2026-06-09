using Data.Items;
using UnityEngine;

namespace Core.Contracts.World
{
    /// <summary>
    /// Interface for spawning loot items using Object Pooling.
    /// </summary>
    public interface ILootFactory
    {
        /// <summary>
        /// Spawns an item pickup at the specified position.
        /// </summary>
        /// <param name="item">The data of the item to spawn.</param>
        /// <param name="position">World position to spawn at.</param>
        /// <param name="parent">Optional parent transform.</param>
        /// <param name="quantity">Amount of item in this pickup.</param>
        /// <param name="elevationLayer">Target sorting layer for elevation (e.g., Elevation_A).</param>
        /// <returns>The spawned GameObject.</returns>
        GameObject SpawnLoot(ItemData item, Vector3 position, Transform parent = null, int quantity = 1, string elevationLayer = null);

        /// <summary>
        /// Spawns multiple loot items with staggered distribution to optimize physics and performance.
        /// </summary>
        void SpawnLootBulk(ItemData item, int count, Vector3 position, Transform parent, float spread, float minForce, float maxForce, string elevationLayer = null);
    }
}
