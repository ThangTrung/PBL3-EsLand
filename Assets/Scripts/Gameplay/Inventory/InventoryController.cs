using System;
using System.Collections.Generic;
using System.Linq;
using Core.Contracts.Inventory;
using Core.Contracts.Shared;
using Data.Items;
using UnityEngine;

namespace Gameplay.Inventory
{
    /// <summary>
    /// Manages the character's inventory state and storage.
    /// Implements IInventory to provide a standard interface for inventory operations.
    /// </summary>
    public class InventoryController : MonoBehaviour, IInventory
    {
        [Header("Settings")]
        [SerializeField] private int capacity = 64;

        private InventorySlot[] _slots;

        public IReadOnlyList<IInventorySlot> Slots => _slots;
        public int Capacity => capacity;
        public int UsedSlots => _slots?.Count(s => !s.IsEmpty) ?? 0;
        
        public IItemActionHandler ActionHandler => GetComponent<IItemActionHandler>() ?? GetComponentInChildren<IItemActionHandler>();

        public event Action OnInventoryChanged;

        private void Awake()
        {
            InitializeSlots();
        }

        private void InitializeSlots()
        {
            if (_slots != null && _slots.Length == capacity) return;
            
            _slots = new InventorySlot[capacity];
            for (var i = 0; i < capacity; i++)
                _slots[i] = new InventorySlot(null, 0);
        }

        public bool AddItem(ItemData item, int amount = 1)
        {
            if (!item || amount <= 0) return false;
            var remaining = amount;

            if (item.MaxStack > 1)
            {
                foreach (var slot in _slots.Where(s => !s.IsEmpty && s.ItemData == item))
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
                var emptySlot = _slots.FirstOrDefault(s => s.IsEmpty);
                if (emptySlot == null)
                {
                    NotifyChanged();
                    return false;
                }
                
                var toAdd = Mathf.Min(item.MaxStack, remaining);
                emptySlot.SetItem(item, toAdd);
                remaining -= toAdd;
            }
            
            NotifyChanged();
            return true;
        }

        public void ConsumeSlot(IInventorySlot slot, int amount = 1)
        {
            if (slot is not InventorySlot concreteSlot) return;
            
            concreteSlot.AddAmount(-amount);
            if (concreteSlot.Amount <= 0) concreteSlot.Clear();
            
            Collapse();
        }

        public bool RemoveSlot(IInventorySlot slot)
        {
            if (slot is not InventorySlot concreteSlot) return false;
            
            concreteSlot.Clear();
            Collapse();
            return true;
        }

        public bool RemoveItem(ItemData item, int amount = 1)
        {
            if (CountItem(item) < amount) return false;
            
            var remaining = amount;
            for (var i = _slots.Length - 1; i >= 0 && remaining > 0; i--)
            {
                if (_slots[i].IsEmpty || _slots[i].ItemData != item) continue;
                
                var take = Mathf.Min(_slots[i].Amount, remaining);
                _slots[i].AddAmount(-take);
                if (_slots[i].Amount <= 0) _slots[i].Clear();
                remaining -= take;
            }
            
            Collapse();
            return true;
        }

        public void Collapse()
        {
            if (_slots == null) return;
            
            int nextFree = 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (!_slots[i].IsEmpty)
                {
                    if (i != nextFree)
                    {
                        _slots[nextFree].SetData(_slots[i].ItemData, _slots[i].Amount, _slots[i].CurrentDurability);
                        _slots[i].Clear();
                    }
                    nextFree++;
                }
            }
            NotifyChanged();
        }

        public int CountItem(ItemData item) =>
            _slots.Where(s => !s.IsEmpty && s.ItemData == item).Sum(s => s.Amount);

        public IInventorySlot GetSlotAt(int index) => 
            (index >= 0 && index < _slots.Length) ? _slots[index] : null;

        public void SwapSlots(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= _slots.Length || indexB < 0 || indexB >= _slots.Length) return;
            (_slots[indexA], _slots[indexB]) = (_slots[indexB], _slots[indexA]);
            NotifyChanged();
        }

        public void Clear()
        {
            if (_slots == null) return;
            foreach (var slot in _slots) slot.Clear();
            NotifyChanged();
        }

        public void NotifyChanged() => OnInventoryChanged?.Invoke();
    }
}
