using System;
using System.Collections.Generic;
using System.Linq;
using Script.Interfaces;
using Script.Items;
using UnityEngine;

namespace Script.Inventory.Controller
{
    public class InventoryController : MonoBehaviour, IInventory
    {
        public static InventoryController Instance { get; private set; }

        [Header("Owner")]
        [SerializeField] private Entities.Character owner;
        public Entities.Character Owner => owner;

        public IItemActionHandler ActionHandler { get; private set; }

        [Header("Settings")] 
        [SerializeField] private int capacity = 64;
        
        private InventorySlot[] _slots;
        
        public IReadOnlyList<IInventorySlot> Slots => _slots;
        public int Capacity => capacity;
        public int UsedSlots => _slots.Count(s => !s.IsEmpty);
        
        public event Action OnInventoryChanged;

        private void Awake()
        {
            if (owner != null && owner.CompareTag("Player"))
            {
                if (Instance == null) Instance = this;
            }
            InitializeSlots();
            ActionHandler = new InventoryActionHandler(this, owner);
        }

        private void InitializeSlots()
        {
            if (_slots != null && _slots.Length == capacity) return;
            _slots = new InventorySlot[capacity];
            for (var i = 0; i < capacity; i++)
            {
                _slots[i] = new InventorySlot(null, 0);
            }
        }

        public void SetOwner(Entities.Character newOwner)
        {
            owner = newOwner;
            ActionHandler = new InventoryActionHandler(this, owner);
        }

        public bool AddItem(Item item, int amount = 1)
        {
            if (!item || amount <= 0) return false;
            var remaining = amount;
            
            // 1. Try to stack with existing items
            if (item.MaxStack > 1)
            {
                foreach (var slot in _slots.Where(s => !s.IsEmpty && s.Item == item))
                {
                    var canAdd = item.MaxStack - slot.Amount;
                    if (canAdd <= 0) continue;
                    
                    var toAdd = Mathf.Min(canAdd, remaining);
                    slot.AddAmount(toAdd);
                    remaining -= toAdd;
                    
                    if (remaining <= 0) break;
                }
            }

            // 2. Add to empty slots
            while (remaining > 0)
            {
                var emptySlot = _slots.FirstOrDefault(s => s.IsEmpty);
                if (emptySlot == null) 
                {
                    RaiseChanged();
                    return false; // Inventory full
                }

                var toAdd = Mathf.Min(item.MaxStack, remaining);
                emptySlot.SetItem(item, toAdd);
                remaining -= toAdd;
            }
            RaiseChanged();
            return true;
        }

        public void ConsumeSlot(IInventorySlot slot, int amount = 1)
        {
            if (slot is not InventorySlot concreteSlot) return;
            concreteSlot.AddAmount(-amount);
            if (concreteSlot.Amount <= 0) concreteSlot.Clear();
            RaiseChanged();
        }

        public bool RemoveSlot(IInventorySlot slot)
        {
            if (slot is not InventorySlot concreteSlot) return false;
            concreteSlot.Clear();
            RaiseChanged();
            return true;
        }

        public bool RemoveItem(Item item, int amount = 1)
        {
            if (CountItem(item) < amount) return false;
            
            var remaining = amount;
            for (var i = _slots.Length - 1; i >= 0 && remaining > 0; i--)
            {
                if (_slots[i].IsEmpty || _slots[i].Item != item) continue;
                
                var take = Mathf.Min(_slots[i].Amount, remaining);
                _slots[i].AddAmount(-take);
                if (_slots[i].Amount <= 0) _slots[i].Clear();
                remaining -= take;
            }
            
            RaiseChanged();
            return true;
        }

        public int CountItem(Item item) => 
            _slots.Where(s => !s.IsEmpty && s.Item == item).Sum(s => s.Amount);

        public IInventorySlot GetSlotAt(int index) => 
            (index >= 0 && index < _slots.Length) ? _slots[index] : null;

        public void SwapSlots(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= _slots.Length || indexB < 0 || indexB >= _slots.Length) return;
            (_slots[indexA], _slots[indexB]) = (_slots[indexB], _slots[indexA]);
            RaiseChanged();
        }

        public void Clear() 
        {
            foreach (var slot in _slots) slot.Clear();
            RaiseChanged();
        }

        public void NotifyChanged() => RaiseChanged();

        private void RaiseChanged() => OnInventoryChanged?.Invoke();
    }
}