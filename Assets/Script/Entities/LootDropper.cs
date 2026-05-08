using Script.Items;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Entities
{
    public class LootDropper : MonoBehaviour
    {
        [Header("Loot Settings")]
        [SerializeField] private List<Item> lootTable = new List<Item>();
        [Range(0, 1)][SerializeField] private float dropChance = 0.5f;

        public void Drop(string sourceName)
        {
            if (lootTable == null || lootTable.Count == 0) return;

            if (Random.value > dropChance) return;

            int idx = Random.Range(0, lootTable.Count);
            var dropped = lootTable[idx];
            if (dropped == null) return;

            Debug.Log($"[Loot] {sourceName} r?i ra v?t ph?m: {dropped.ItemName}");

            // TODO: Instantiate pickup prefab (v?t ph?m r?i ra ngoài world)
        }
    }
}
