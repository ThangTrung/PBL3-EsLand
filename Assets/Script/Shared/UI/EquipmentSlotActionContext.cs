using Script.Equipment.Core;
using Script.Equipment.Interfaces;
using Script.Items;
using Script.Shared.Interfaces;

namespace Script.Shared.UI
{
    /// <summary>
    /// Adapter: wraps an equipped IEquippable + IEquipmentManager into IActionableItem
    /// so the shared ItemActionMenu has no dependency on the Equipment system.
    /// </summary>
    public class EquipmentSlotActionContext : IActionableItem
    {
        private readonly IEquipmentManager _manager;
        private readonly EquipSlot _slot;
        private readonly IEquippable _item;

        public EquipmentSlotActionContext(IEquipmentManager manager, EquipSlot slot, IEquippable item)
        {
            _manager = manager;
            _slot = slot;
            _item = item;
        }

        public string DisplayName => (_item as Script.Items.Item)?.ItemName ?? "Unknown";
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
