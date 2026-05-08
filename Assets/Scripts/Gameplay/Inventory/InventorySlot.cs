using System;
using Core.Contracts.Equipment;
using Core.Contracts.Inventory;
using Core.Contracts.Shared;
using Data.Items;

namespace Gameplay.Inventory
{
    [Serializable]
    public class InventorySlot : IInventorySlot
    {
        public Item Item { get; private set; }
        public int Amount { get; private set; }
        public int CurrentDurability { get; private set; }
        public bool IsEmpty => Item == null || Amount <= 0;

        public float DurabilityPercent => Item is IDurable { MaxDurability: > 0 } durable
            ? (float)CurrentDurability / durable.MaxDurability
            : 1f;

        public InventorySlot(Item newItem, int startAmount)
        {
            Item = newItem;
            Amount = Math.Max(0, startAmount);
            CurrentDurability = newItem is IDurable durable ? durable.MaxDurability : 0;
        }

        public void AddAmount(int delta)
        {
            Amount += delta;
            if (Amount < 0) Amount = 0;
        }

        public void ReduceDurability(int amount)
        {
            if (Item is not IDurable) return;
            CurrentDurability -= amount;
            if (CurrentDurability < 0) CurrentDurability = 0;
        }

        public void RepairDurability(int amount)
        {
            if (Item is not IDurable durable) return;
            CurrentDurability += amount;
            if (CurrentDurability > durable.MaxDurability)
                CurrentDurability = durable.MaxDurability;
        }

        public void SetItem(Item item, int amount)
        {
            Item = item;
            Amount = amount;
            CurrentDurability = item is IDurable durable ? durable.MaxDurability : 0;
        }

        public void Clear()
        {
            Item = null;
            Amount = 0;
            CurrentDurability = 0;
        }
    }
}


