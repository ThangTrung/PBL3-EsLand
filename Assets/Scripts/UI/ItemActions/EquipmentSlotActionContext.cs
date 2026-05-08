using Core.Contracts.Equipment;
using Core.Contracts.Shared;
using Data.Equipment;
using Data.Items;

namespace UI.ItemActions
{
    /// <summary>
    /// Adapter: wraps an equipped IEquippable + IEquipmentController into IActionableItem
    /// so the shared ItemActionMenu has no dependency on the Equipment system.
    /// </summary>
    public class EquipmentSlotActionContext : IActionableItem
    {
        private readonly IEquipmentController _manager;
        private readonly EquipSlot _slot;
        private readonly IEquippable _item;

        public EquipmentSlotActionContext(IEquipmentController manager, EquipSlot slot, IEquippable item)
        {
            _manager = manager;
            _slot = slot;
            _item = item;
        }

        public string DisplayName => (_item as Item)?.ItemName ?? "Unknown";
        public bool CanUse     => false;
        public bool CanDrop    => false;
        public bool CanEquip   => false;
        public bool CanUnequip => _item != null;

        public void Use()     { }
        public void Drop()    { }
        public void Equip()   { }
        public void Unequip() => _manager?.Unequip(_slot);
    }
}



