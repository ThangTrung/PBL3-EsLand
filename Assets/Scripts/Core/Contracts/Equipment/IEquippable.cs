using Data.Equipment;
using Gameplay.Characters;

namespace Core.Contracts.Equipment
{
    public interface IEquippable
    {
        EquipSlot Slot { get; }
        void OnEquip(Character character);
        void OnUnequip(Character character);
    }
}

