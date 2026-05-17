using System;
using System.Collections.Generic;
using Core.Contracts.Equipment;
using Core.Contracts.Shared;
using Data.Equipment;
using Gameplay.Characters;
using UnityEngine;

namespace Gameplay.Equipment
{
    public class EquipmentManager : MonoBehaviour, IEquipmentController
    {
        private readonly Dictionary<EquipSlot, IEquippable> _equippedItems = new Dictionary<EquipSlot, IEquippable>();
        private Character _character;

        public event Action<EquipSlot, IEquippable> OnItemEquipped;
        public event Action<EquipSlot, IEquippable> OnItemUnequipped;

        public IReadOnlyDictionary<EquipSlot, IEquippable> EquippedItems => _equippedItems;

        public void Initialize(Character character)
        {
            _character = character;
        }

        public void Equip(IEquippable item)
        {
            if (item == null) return;
            if (_equippedItems.ContainsKey(item.Slot))
                Unequip(item.Slot);
            _equippedItems[item.Slot] = item;
            item.OnEquip(_character);
            OnItemEquipped?.Invoke(item.Slot, item);
        }

        public void Unequip(EquipSlot slot)
        {
            if (!_equippedItems.TryGetValue(slot, out var item))
                return;
            item.OnUnequip(_character);
            _equippedItems.Remove(slot);
            OnItemUnequipped?.Invoke(slot, item);
        }

        public float GetTotalDamageModifier()
        {
            float total = 0;
            foreach (var item in _equippedItems.Values)
                if (item is IStatModifierProvider provider)
                    total += provider.GetDamageModifier();
            return total;
        }

        public float GetTotalDefenseModifier()
        {
            float total = 0;
            foreach (var item in _equippedItems.Values)
                if (item is IStatModifierProvider provider)
                    total += provider.GetDefenseModifier();
            return total;
        }

        public float GetTotalSpeedModifier()
        {
            float total = 0;
            foreach (var item in _equippedItems.Values)
                if (item is IStatModifierProvider provider)
                    total += provider.GetSpeedModifier();
            return total;
        }

        public float GetTotalHealthModifier()
        {
            float total = 0;
            foreach (var item in _equippedItems.Values)
                if (item is IStatModifierProvider provider)
                    total += provider.GetHealthModifier();
            return total;
        }

        public IEquippable GetEquippedItem(EquipSlot slot)
        {
            _equippedItems.TryGetValue(slot, out var item);
            return item;
        }
    }
}



