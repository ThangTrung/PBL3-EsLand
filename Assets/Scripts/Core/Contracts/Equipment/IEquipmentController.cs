using System;
using System.Collections.Generic;
using Data.Equipment;
using Gameplay.Characters;

namespace Core.Contracts.Equipment
{
    public interface IEquipmentController
    {
        float GetTotalDamageModifier();
        float GetTotalHealthModifier();
        float GetTotalDefenseModifier();
        float GetTotalSpeedModifier();
        void Equip(IEquippable item);
        void Unequip(EquipSlot slot);
        void Initialize(Character owner);
        IEquippable GetEquippedItem(EquipSlot slot);
        IReadOnlyDictionary<EquipSlot, IEquippable> EquippedItems { get; }
        event Action<EquipSlot, IEquippable> OnItemEquipped;
        event Action<EquipSlot, IEquippable> OnItemUnequipped;
    }
}


