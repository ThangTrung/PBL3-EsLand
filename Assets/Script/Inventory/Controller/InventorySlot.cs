using System;
using Script.Items;

namespace Script.Inventory.Controller
{
    [Serializable]
    public class InventorySlot
    {
        public Item Item { get; }
        public int Amount { get; private set; }
        public int CurrentDurability { get; private set; }
        public bool IsEmpty => Item == null || Amount <= 0;
        public bool IsEquipment => Item is Equipment;
        public float DurabilityPercent => IsEquipment && Item is Equipment e
            ? (float)CurrentDurability / e.MaxDurability
            : 1f;

        public InventorySlot(Item newItem, int startAmount)
        {
            Item = newItem;
            Amount = Math.Max(1, startAmount);
            CurrentDurability = newItem is Equipment eq ? eq.MaxDurability : 0;
        }

        public void AddAmount(int delta)
        {
            Amount += delta;
            if (Amount < 0) Amount = 0;
        }

        public void ReduceDurability(int amount)
        {
            CurrentDurability -= amount;
            if (CurrentDurability < 0) CurrentDurability = 0;
        }

        public void RepairDurability(int amount)
        {
            if (Item is not Equipment eq) 
                return;
            CurrentDurability += amount;
            if (CurrentDurability > eq.MaxDurability)
                CurrentDurability = eq.MaxDurability;
        }
    }
}
