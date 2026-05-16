using System.Collections.Generic;
using Data.Items;
using UnityEngine;

namespace Gameplay.World
{
    public class LootSpawner : MonoBehaviour
    {
        [Header("Loot Settings")]
        [SerializeField] private GameObject pickupPrefab;
        [SerializeField] private List<LootItem> lootTable;
        [SerializeField] private float dropForce = 5f;

        [System.Serializable]
        public class LootItem
        {
            public Item item;
            public int minAmount = 1;
            public int maxAmount = 3;
            [Range(0, 1)] public float chance = 1f;
        }

        public void SpawnLoot()
        {
            if (!pickupPrefab || lootTable == null) return;

            foreach (var loot in lootTable)
            {
                if (Random.value > loot.chance) continue;

                var count = Random.Range(loot.minAmount, loot.maxAmount + 1);
                for (var i = 0; i < count; i++)
                {
                    DropItem(loot.item);
                }
            }
        }

        private void DropItem(Item item)
        {
            if (!item) return;

            var droppedObj = Instantiate(pickupPrefab, transform.position, Quaternion.identity);
            
            if (droppedObj.TryGetComponent<ItemPickup>(out var pickupScript))
            {
                pickupScript.itemData = item;
            }

            if (!droppedObj.TryGetComponent<Rigidbody2D>(out var rb)) 
                return;
            var randomDir = Random.insideUnitCircle.normalized;
            rb.AddForce(randomDir * dropForce, ForceMode2D.Impulse);
        }
    }
}

