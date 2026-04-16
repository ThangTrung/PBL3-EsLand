using System;
using System.Collections.Generic;
using System.Linq;
using Script.Items;
using UnityEngine;

using Script.Inventory.UI;

namespace Script.Inventory.Controller
{
    public class InventoryController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int capacity = 30;
        
        [Header("UI Reference")]
        [SerializeField] private InventoryUI inventoryUI;
        
        private readonly List<InventorySlot> _slots = new List<InventorySlot>();
        public IReadOnlyList<InventorySlot> Slots => _slots;
        public int Capacity => capacity;
        public int UsedSlots => _slots.Count;
        private bool IsFull => _slots.Count >= capacity;
        private bool IsVisible { get; set; }

        public event Action OnInventoryChanged;
        public event Action<bool> OnVisibilityChanged;

        private void Start()
        {
            inventoryUI.Setup(this);
        }

        private void Display()
        {
            if (IsVisible) 
                return;
            IsVisible = true;
            OnVisibilityChanged?.Invoke(true);
        }

        private void Hide()
        {
            if (!IsVisible) return;
            IsVisible = false;
            OnVisibilityChanged?.Invoke(false);
        }

        public void ToggleDisplay()
        {
            if (IsVisible) Hide(); else Display();
        }

        public bool AddItem(Item item, int amount = 1)
        {
            if (item == null || amount <= 0) 
                return false;
            var rem = amount;
            if (item.MaxStack > 1)
            {
                foreach (var slot in _slots)
                {
                    if (slot.Item != item) 
                        continue;
                    var canAdd = item.MaxStack - slot.Amount;
                    if (canAdd <= 0) 
                        continue;
                    var add = Mathf.Min(canAdd, rem);
                    slot.AddAmount(add);
                    rem -= add;
                    if (rem <= 0) break;
                }
            }
            while (rem > 0)
            {
                if (IsFull)
                {
                    RaiseChanged(); 
                    return false; 
                }
                var add = Mathf.Min(item.MaxStack, rem);
                _slots.Add(new InventorySlot(item, add));
                rem -= add;
            }
            RaiseChanged();
            return true;
        }

        public bool RemoveSlot(InventorySlot slot)
        {
            if (!_slots.Contains(slot)) return false;
            _slots.Remove(slot);
            RaiseChanged();
            return true;
        }

        private void ConsumeSlot(InventorySlot slot, int amount = 1)
        {
            if (!_slots.Contains(slot)) return;
            slot.AddAmount(-amount);
            if (slot.Amount <= 0) _slots.Remove(slot);
            RaiseChanged();
        }

        public bool RemoveItem(Item item, int amount = 1)
        {
            if (!HasItem(item, amount)) 
                return false;
            var rem = amount;
            for (var i = _slots.Count - 1; i >= 0 && rem > 0; i--)
            {
                if (_slots[i].Item != item) continue;
                var take = Mathf.Min(_slots[i].Amount, rem);
                _slots[i].AddAmount(-take);
                rem -= take;
                if (_slots[i].Amount <= 0) _slots.RemoveAt(i);
            }
            RaiseChanged();
            return true;
        }

        // public void UseItem(InventorySlot slot, Character user)
        // {
        //     if (slot == null || slot.Item == null) return;
        //     var used = slot.Item.Use(user);
        //     if (!used) 
        //         return;
        //     var isEquip = slot.Item is Equipment;
        //     if (!isEquip) ConsumeSlot(slot); else RaiseChanged();
        // }

        private bool HasItem(Item item, int amount = 1)
        {
            var total = _slots.Where(s => s.Item == item).Sum(s => s.Amount);
            return total >= amount;
        }

        public int CountItem(Item item)
        {
            return _slots.Where(s => s.Item == item).Sum(s => s.Amount);
        }

        public InventorySlot GetSlot(Item item) => _slots.Find(s => s.Item == item);

        public InventorySlot GetSlotAt(int index) =>
            (index >= 0 && index < _slots.Count) ? _slots[index] : null;

        public void SwapSlots(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= _slots.Count) return;
            if (indexB < 0 || indexB >= _slots.Count) return;
            (_slots[indexA], _slots[indexB]) = (_slots[indexB], _slots[indexA]);
            RaiseChanged();
        }

        public void Clear() { _slots.Clear(); RaiseChanged(); }

        private void RaiseChanged() => OnInventoryChanged?.Invoke();
    }
}
