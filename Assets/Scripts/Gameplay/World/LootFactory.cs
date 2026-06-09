using System.Collections;
using Core.Contracts.World;
using Data.Items;
using Infrastructure.Pooling;
using UnityEngine;
using Gameplay.Environment;

namespace Gameplay.World
{
    /// <summary>
    /// Implementation of ILootFactory using ObjectPoolManager.
    /// Handles the visual and data initialization of loot pickups.
    /// </summary>
    public class LootFactory : MonoBehaviour, ILootFactory
    {
        [Header("Settings")]
        [SerializeField] private GameObject genericPickupPrefab;

        public static LootFactory Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (genericPickupPrefab != null)
                {
                    ObjectPoolManager.Instance.InitPool(genericPickupPrefab, 20, 500);
                }
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public GameObject SpawnLoot(ItemData item, Vector3 position, Transform parent = null, int quantity = 1, string elevationLayer = null)
        {
            if (item == null || genericPickupPrefab == null)
            {
                Debug.LogWarning("[LootFactory] Item or Prefab is missing.");
                return null;
            }

            // [MANDATE] Use ObjectPoolManager instead of Instantiate
            GameObject droppedObj = ObjectPoolManager.Instance.Get(genericPickupPrefab, position, Quaternion.identity, parent);
            
            // [FIX] Elevation Context Propagation
            if (!string.IsNullOrEmpty(elevationLayer) && droppedObj.TryGetComponent<ElevationAgent>(out var elevationAgent))
            {
                elevationAgent.ChangeElevation(elevationLayer);
            }

            // Setup Sprite Renderer
            var spriteRenderer = droppedObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = droppedObj.GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer != null && item.Icon != null)
            {
                spriteRenderer.sprite = item.Icon;
            }

            // Setup Item Data & Quantity
            if (droppedObj.TryGetComponent<ItemPickup>(out var pickupScript))
            {
                pickupScript.SetItem(item, quantity);
            }
            else
            {
                Debug.LogError($"[LootFactory] {genericPickupPrefab.name} is missing ItemPickup component.");
            }

            return droppedObj;
        }

        public void SpawnLootBulk(ItemData item, int count, Vector3 position, Transform parent, float spread, float minForce, float maxForce, string elevationLayer = null)
        {
            if (count <= 0 || item == null) return;

            // [PHASE 2] Stacking Logic for high volume drops
            // If dropping more than 20 items of a stackable type, group them into larger stacks
            if (count > 20 && item.MaxStack > 1)
            {
                int itemsPerStack = Mathf.Min(item.MaxStack, 10); // Group into stacks of 10 or MaxStack
                int stackCount = Mathf.CeilToInt((float)count / itemsPerStack);
                
                StartCoroutine(SpawnStaggeredRoutine(item, stackCount, position, parent, spread, minForce, maxForce, itemsPerStack, count % itemsPerStack, elevationLayer));
            }
            else if (count > 10)
            {
                StartCoroutine(SpawnStaggeredRoutine(item, count, position, parent, spread, minForce, maxForce, 1, 0, elevationLayer));
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    DropSingle(item, position, parent, spread, minForce, maxForce, 1, elevationLayer);
                }
            }
        }

        private IEnumerator SpawnStaggeredRoutine(ItemData item, int count, Vector3 position, Transform parent, float spread, float minForce, float maxForce, int qtyPerPickup, int remainder, string elevationLayer)
        {
            int spawned = 0;
            while (spawned < count)
            {
                int batch = Mathf.Min(5, count - spawned);
                for (int i = 0; i < batch; i++)
                {
                    int currentQty = (spawned == count - 1 && remainder > 0) ? remainder : qtyPerPickup;
                    DropSingle(item, position, parent, spread, minForce, maxForce, currentQty, elevationLayer);
                    spawned++;
                }
                yield return new WaitForSeconds(0.05f);
            }
        }

        private void DropSingle(ItemData item, Vector3 position, Transform parent, float spread, float minForce, float maxForce, int quantity, string elevationLayer)
        {
            Vector3 randomOffset = (Vector3)Random.insideUnitCircle * spread;
            var obj = SpawnLoot(item, position + randomOffset, parent, quantity, elevationLayer);
            
            if (obj != null && obj.TryGetComponent<Rigidbody2D>(out var rb))
            {
                var randomDir = Random.insideUnitCircle.normalized;
                var force = Random.Range(minForce, maxForce);
                rb.AddForce(randomDir * force, ForceMode2D.Impulse);
            }
        }
    }
}
