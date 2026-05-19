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
                private readonly IItemActionHandler _handler;
        private readonly EquipSlot _slot;
        private readonly IEquippable _item;

                public EquipmentSlotActionContext(IItemActionHandler handler, EquipSlot slot, IEquippable item)
        {
                        _handler = handler;
            _slot = slot;
            _item = item;
        }

        public string DisplayName => (_item as ItemData)?.ItemName ?? "Unknown";
        public bool CanUse     => false;
                public bool CanDrop    => _item != null;
        public bool CanEquip   => false;
        public bool CanUnequip => _item != null;

        public void Use()     { }
public void Drop()    => _handler?.DropEquippedItem(_slot);
        public void Equip()   { }
public void Unequip() => _handler?.UnequipItem(_slot);
    }
}



