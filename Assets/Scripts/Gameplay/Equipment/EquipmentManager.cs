using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Initialize(Character character) => _character = character;

        public void Equip(IEquippable item)
        {
            if (item == null) return;
            if (_equippedItems.ContainsKey(item.Slot)) Unequip(item.Slot);
            
            _equippedItems[item.Slot] = item;
            item.OnEquip(_character);
            OnItemEquipped?.Invoke(item.Slot, item);
        }

        public void Unequip(EquipSlot slot)
        {
            if (!_equippedItems.TryGetValue(slot, out var item)) return;
            
            item.OnUnequip(_character);
            _equippedItems.Remove(slot);
            OnItemUnequipped?.Invoke(slot, item);
        }

        public float GetTotalDamageModifier() => _equippedItems.Values.OfType<IStatModifierProvider>().Sum(p => p.GetDamageModifier());
        public float GetTotalDefenseModifier() => _equippedItems.Values.OfType<IStatModifierProvider>().Sum(p => p.GetDefenseModifier());
        public float GetTotalSpeedModifier() => _equippedItems.Values.OfType<IStatModifierProvider>().Sum(p => p.GetSpeedModifier());
        public float GetTotalHealthModifier() => _equippedItems.Values.OfType<IStatModifierProvider>().Sum(p => p.GetHealthModifier());

        public IEquippable GetEquippedItem(EquipSlot slot)
        {
            _equippedItems.TryGetValue(slot, out var item);
            return item;
        }
    }
}
