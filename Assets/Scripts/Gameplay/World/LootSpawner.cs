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
        [SerializeField] private float minDropForce = 4f;
        [SerializeField] private float maxDropForce = 8f;

        [System.Serializable]
        public class LootItem
        {
            public ItemData item;
            public int minAmount = 1;
            public int maxAmount = 3;
            [Range(0, 1)] public float chance = 1f;
        }

        public void SetLoot(ItemData item, int amount)
        {
            lootTable = new List<LootItem>
            {
                new LootItem { item = item, minAmount = amount, maxAmount = amount, chance = 1f }
            };
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

        private void DropItem(ItemData item)
        {
            if (!item) return;

            var droppedObj = Instantiate(pickupPrefab, transform.position, Quaternion.identity);
            
            // [FIX] Sử dụng cơ chế ElevationAgent trên Prefab gốc để định vị Layer
            // Chỉ cần gán Sprite đúng với Icon của ItemData
            var spriteRenderer = droppedObj.GetComponent<SpriteRenderer>() ?? droppedObj.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                if (item.Icon != null)
                {
                    spriteRenderer.sprite = item.Icon;
                }
            }

            if (droppedObj.TryGetComponent<ItemPickup>(out var pickupScript))
            {
                pickupScript.itemData = item;
            }

            if (!droppedObj.TryGetComponent<Rigidbody2D>(out var rb)) 
                return;
            var randomDir = Random.insideUnitCircle.normalized;
            var force = Random.Range(minDropForce, maxDropForce);
            rb.AddForce(randomDir * force, ForceMode2D.Impulse);
        }
    }
}
