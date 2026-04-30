using System;
using System.Collections.Generic;
using System.Linq;
using Script.Equipment.Interfaces;
using Script.Inventory.Interfaces;
using Script.Items;
using Script.Shared.Interfaces;
using UnityEngine;

namespace Script.Inventory.Core
{
    [RequireComponent(typeof(Entities.Character))]
    public class InventoryManager : MonoBehaviour, IInventory, IItemActionHandler
    {
        public static InventoryManager Instance { get; private set; }

        public Entities.Character Owner { get; private set; }

        // Self-reference: InventoryController implements IItemActionHandler directly
        public IItemActionHandler ActionHandler => this;

        [Header("Settings")]
        [SerializeField] private int capacity = 64;

        private InventorySlot[] _slots;

        public IReadOnlyList<IInventorySlot> Slots => _slots;
        public int Capacity => capacity;
        public int UsedSlots => _slots.Count(s => !s.IsEmpty);

        public event Action OnInventoryChanged;

        private void Awake()
        {
            Owner = GetComponent<Entities.Character>();

            if (Owner != null && Owner.CompareTag("Player"))
            {
                if (Instance == null) Instance = this;
            }

            InitializeSlots();
        }

        private void InitializeSlots()
        {
            if (_slots != null && _slots.Length == capacity) return;
            _slots = new InventorySlot[capacity];
            for (var i = 0; i < capacity; i++)
                _slots[i] = new InventorySlot(null, 0);
        }

        public void SetOwner(Entities.Character newOwner)
        {
            Owner = newOwner;
        }

        public bool AddItem(Item item, int amount = 1)
        {
            if (!item || amount <= 0) return false;
            var remaining = amount;

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

            while (remaining > 0)
            {
                var emptySlot = _slots.FirstOrDefault(s => s.IsEmpty);
                if (emptySlot == null)
                {
                    RaiseChanged();
                    return false;
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

        // ── IItemActionHandler (merged from InventoryActionHandler) ──────────

        public void UseItem(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty || Owner == null) return;
            if (slot.Item is IItemUsable usable && usable.Use(Owner))
                ConsumeSlot(slot, 1);
        }

        public void DropItem(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty) return;
            slot.Item.Drop();
            RemoveSlot(slot);
        }

        public void EquipItem(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty || Owner == null) return;
            if (slot.Item is not IEquippable equippable) return;
            if (slot.Item is IDurable && slot.CurrentDurability <= 0)
            {
                Debug.LogWarning("Item is broken, cannot equip!");
                return;
            }
            if (Owner is IInventoryHolder holder)
                holder.EquipmentManager?.Equip(equippable);
            NotifyChanged();
        }

        public void UnequipItem(EquipSlot slot)
        {
            if (Owner is IInventoryHolder holder)
                holder.EquipmentManager?.Unequip(slot);
            NotifyChanged();
        }

        public bool IsEquipped(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty || Owner is not IInventoryHolder holder) return false;
            if (slot.Item is not IEquippable equippable) return false;
            var equipped = holder.EquipmentManager?.GetEquippedItem(equippable.Slot);
            return equipped != null && ReferenceEquals(equipped, slot.Item);
        }
    }
}
