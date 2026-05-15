using System;
using System.Collections.Generic;
using System.Linq;
using Core.Contracts.Inventory;
using Core.Contracts.Shared;
using Data.Items;
using UnityEngine;

namespace Gameplay.Inventory
{
    [RequireComponent(typeof(InventoryContainer))]
    public class InventoryController : MonoBehaviour, IInventory
    {
        private InventoryContainer _container;

        public IReadOnlyList<IInventorySlot> Slots => _container.Slots;
        public int Capacity => _container.Capacity;
        public int UsedSlots => _container.UsedSlots;
        public IItemActionHandler ActionHandler => GetComponent<IItemActionHandler>();

        public event Action OnInventoryChanged
        {
            add => _container.OnInventoryChanged += value;
            remove => _container.OnInventoryChanged -= value;
        }

        private void Awake()
        {
            _container = GetComponent<InventoryContainer>();
        }

        public bool AddItem(Item item, int amount = 1)
        {
            if (!item || amount <= 0) return false;
            var remaining = amount;

            var allSlots = _container.GetAllSlots().ToArray();

            if (item.MaxStack > 1)
            {
                foreach (var slot in allSlots.Where(s => !s.IsEmpty && s.Item == item))
                {
                    var canAdd = item.MaxStack - slot.Amount;
                    if (canAdd <= 0) continue;
                    var toAdd = Mathf.Min(canAdd, remaining);
                    slot.AddAmount(toAdd);
                    remaining -= toAdd;
                    if (remaining <= 0) break;
                }
            }

            while (remaining > 0)
            {
                var emptySlot = allSlots.FirstOrDefault(s => s.IsEmpty);
                if (emptySlot == null)
                {
                    _container.NotifyChanged();
                    return false;
                }
                var toAdd = Mathf.Min(item.MaxStack, remaining);
                emptySlot.SetItem(item, toAdd);
                remaining -= toAdd;
            }
            
            _container.NotifyChanged();
            return true;
        }

        public void ConsumeSlot(IInventorySlot slot, int amount = 1)
        {
            if (slot is not InventorySlot concreteSlot) return;
            concreteSlot.AddAmount(-amount);
            if (concreteSlot.Amount <= 0) concreteSlot.Clear();
            _container.Collapse();
        }

        public bool RemoveSlot(IInventorySlot slot)
        {
            if (slot is not InventorySlot concreteSlot) return false;
            concreteSlot.Clear();
            _container.Collapse();
            return true;
        }

        public bool RemoveItem(Item item, int amount = 1)
        {
            if (CountItem(item) < amount) return false;
            var remaining = amount;
            var allSlots = _container.GetAllSlots().ToArray();

            for (var i = allSlots.Length - 1; i >= 0 && remaining > 0; i--)
            {
                if (allSlots[i].IsEmpty || allSlots[i].Item != item) continue;
                var take = Mathf.Min(allSlots[i].Amount, remaining);
                allSlots[i].AddAmount(-take);
                if (allSlots[i].Amount <= 0) allSlots[i].Clear();
                remaining -= take;
            }
            
            _container.Collapse();
            return true;
        }

        public int CountItem(Item item) =>
            _container.GetAllSlots().Where(s => !s.IsEmpty && s.Item == item).Sum(s => s.Amount);

        public IInventorySlot GetSlotAt(int index) => _container.GetSlotAt(index);

        public void SwapSlots(int indexA, int indexB) => _container.SwapSlots(indexA, indexB);

        public void Clear() => _container.Clear();

        public void NotifyChanged() => _container.NotifyChanged();
    }
}
