using System;
using System.Collections.Generic;

namespace Script.Equipment.Interfaces
{
    public interface IEquipmentManager
    {
        float GetTotalDamageModifier();
        float GetTotalHealthModifier();
        float GetTotalDefenseModifier();
        float GetTotalSpeedModifier();
        void Equip(Script.Equipment.Interfaces.IEquippable item);
        void Unequip(Script.Items.EquipSlot slot);
        void Initialize(Script.Entities.Character owner);
        Script.Equipment.Interfaces.IEquippable GetEquippedItem(Script.Items.EquipSlot slot);
        IReadOnlyDictionary<Script.Items.EquipSlot, Script.Equipment.Interfaces.IEquippable> EquippedItems { get; }
        event Action<Script.Items.EquipSlot, Script.Equipment.Interfaces.IEquippable> OnItemEquipped;
        event Action<Script.Items.EquipSlot, Script.Equipment.Interfaces.IEquippable> OnItemUnequipped;
    }
}
