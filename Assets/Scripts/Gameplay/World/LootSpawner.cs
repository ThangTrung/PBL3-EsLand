using System.Collections;
using System.Collections.Generic;
using Data.Items;
using UnityEngine;
using Gameplay.Environment;

namespace Gameplay.World
{
    /// <summary>
    /// Handles the spawning of loot items in the world.
    /// Refactored to use LootFactory (Pooling) and support high-volume drops.
    /// </summary>
    public class LootSpawner : MonoBehaviour
    {
        [Header("Loot Settings")]
        [SerializeField] private List<LootItem> lootTable;
        [SerializeField] private float minDropForce = 4f;
        [SerializeField] private float maxDropForce = 8f;
        [SerializeField] private float spawnSpread = 0.5f;

        private ElevationAgent _elevationAgent;

        private void Awake()
        {
            _elevationAgent = GetComponent<ElevationAgent>();
        }

        [System.Serializable]
        public class LootItem
        {
            public ItemData item;
            public int minAmount = 1;
            public int maxAmount = 3;
            [Range(0, 1)] public float chance = 1f;
        }

        public void ClearLoot()
        {
            if (lootTable == null) lootTable = new List<LootItem>();
            else lootTable.Clear();
        }

        public void AddLoot(ItemData item, int min, int max, float chance)
        {
            if (lootTable == null) lootTable = new List<LootItem>();
            lootTable.Add(new LootItem { item = item, minAmount = min, maxAmount = max, chance = chance });
        }

        public void SpawnLoot()
        {
            if (LootFactory.Instance == null || lootTable == null) return;

            string targetLayer = _elevationAgent != null ? _elevationAgent.CurrentElevation : null;

            foreach (var loot in lootTable)
            {
                if (Random.value > loot.chance) continue;

                var count = Random.Range(loot.minAmount, loot.maxAmount + 1);
                
                // [SENIOR FIX] Delegate bulk/staggered spawning to persistent factory with elevation propagation
                LootFactory.Instance.SpawnLootBulk(loot.item, count, transform.position, transform.parent, spawnSpread, minDropForce, maxDropForce, targetLayer);
            }
        }

        private void DropItem(ItemData item)
        {
            // Logic moved to SpawnLootBulk or kept for single drops if needed
            // But for consistency, let's use Bulk for everything or keep this for manual calls.
            if (!item || LootFactory.Instance == null) return;

            string targetLayer = _elevationAgent != null ? _elevationAgent.CurrentElevation : null;

            Vector3 randomOffset = (Vector3)Random.insideUnitCircle * spawnSpread;
            Vector3 spawnPos = transform.position + randomOffset;
            var droppedObj = LootFactory.Instance.SpawnLoot(item, spawnPos, transform.parent, 1, targetLayer);
            ApplyLootForce(droppedObj);
        }

        private void ApplyLootForce(GameObject droppedObj)
        {
            if (droppedObj != null && droppedObj.TryGetComponent<Rigidbody2D>(out var rb))
            {
                var randomDir = Random.insideUnitCircle.normalized;
                var force = Random.Range(minDropForce, maxDropForce);
                rb.AddForce(randomDir * force, ForceMode2D.Impulse);
            }
        }
    }
}
