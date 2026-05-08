using System;
using System.Collections.Generic;
using System.Linq;
using Core.Contracts.Inventory;
using UnityEngine;

namespace Gameplay.Inventory
{
    public class InventoryContainer : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int capacity = 64;

        private InventorySlot[] _slots;

        public IReadOnlyList<IInventorySlot> Slots => _slots;
        public int Capacity => capacity;
        public int UsedSlots => _slots?.Count(s => !s.IsEmpty) ?? 0;

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

        public IInventorySlot GetSlotAt(int index) =>
            (index >= 0 && index < _slots.Length) ? _slots[index] : null;

        public InventorySlot GetConcreteSlotAt(int index) =>
            (index >= 0 && index < _slots.Length) ? _slots[index] : null;

        public IEnumerable<InventorySlot> GetAllSlots() => _slots;

        public void SwapSlots(int indexA, int indexB)
        {
            if (indexA < 0 || indexA >= _slots.Length || indexB < 0 || indexB >= _slots.Length) return;
            (_slots[indexA], _slots[indexB]) = (_slots[indexB], _slots[indexA]);
            NotifyChanged();
        }

        public void Clear()
        {
            foreach (var slot in _slots) slot.Clear();
            NotifyChanged();
        }

        public void NotifyChanged() => OnInventoryChanged?.Invoke();
    }
}
