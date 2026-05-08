namespace Script.Equipment.Interfaces
{
    public interface IEquippable
    {
        Items.EquipSlot Slot { get; }
        void OnEquip(Entities.Character character);
        void OnUnequip(Entities.Character character);
    }
}
