using System;
using System.Collections.Generic;
using Script.Interfaces;
using Script.Items;
using UnityEngine;

namespace Script.Entities
{
    public class EquipmentManager : MonoBehaviour
    {
        private readonly Dictionary<EquipSlot, IEquippable> _equippedItems = new Dictionary<EquipSlot, IEquippable>();
        private Character _character;

        public event Action<EquipSlot, IEquippable> OnItemEquipped;
        public event Action<EquipSlot, IEquippable> OnItemUnequipped;

        public void Initialize(Character character)
        {
            _character = character;
        }

        public void Equip(IEquippable item)
        {
            if (item == null) return;

            if (_equippedItems.ContainsKey(item.EquipSlot))
            {
                Unequip(item.EquipSlot);
            }

            _equippedItems[item.EquipSlot] = item;
            item.OnEquip(_character);
            OnItemEquipped?.Invoke(item.EquipSlot, item);
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
            {
                if (item is IStatModifierProvider provider)
                    total += provider.GetDamageModifier();
            }
            return total;
        }

        public float GetTotalDefenseModifier()
        {
            float total = 0;
            foreach (var item in _equippedItems.Values)
            {
                if (item is IStatModifierProvider provider)
                    total += provider.GetDefenseModifier();
            }
            return total;
        }

        public float GetTotalSpeedModifier()
        {
            float total = 0;
            foreach (var item in _equippedItems.Values)
            {
                if (item is IStatModifierProvider provider)
                    total += provider.GetSpeedModifier();
            }
            return total;
        }

        public float GetTotalHealthModifier()
        {
            float total = 0;
            foreach (var item in _equippedItems.Values)
            {
                if (item is IStatModifierProvider provider)
                    total += provider.GetHealthModifier();
            }
            return total;
        }

        public IEquippable GetEquippedItem(EquipSlot slot)
        {
            _equippedItems.TryGetValue(slot, out var item);
            return item;
        }
    }
}
