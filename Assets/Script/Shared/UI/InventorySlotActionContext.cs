using Script.Equipment.Interfaces;
using Script.Inventory.Interfaces;
using Script.Items;
using Script.Shared.Interfaces;

namespace Script.Shared.UI
{
    /// <summary>
    /// Adapter: wraps an IInventorySlot + IItemActionHandler into IActionableItem
    /// so the shared ItemActionMenu has no dependency on the Inventory system.
    /// </summary>
    public class InventorySlotActionContext : IActionableItem
    {
        private readonly IInventorySlot _slot;
        private readonly IItemActionHandler _handler;

        public InventorySlotActionContext(IInventorySlot slot, IItemActionHandler handler)
        {
            _slot = slot;
            _handler = handler;
        }

        public string DisplayName => _slot?.Item?.ItemName ?? string.Empty;
        public bool CanUse     => _slot?.Item is IItemUsable;
        public bool CanDrop    => true;
        public bool CanEquip   => _slot?.Item is IEquippable && !(_handler?.IsEquipped(_slot) ?? false);
        public bool CanUnequip => _slot?.Item is IEquippable && (_handler?.IsEquipped(_slot) ?? false);

        public void Use()     => _handler?.UseItem(_slot);
        public void Drop()    => _handler?.DropItem(_slot);
        public void Equip()   => _handler?.EquipItem(_slot);
        public void Unequip()
        {
            if (_slot?.Item is IEquippable equippable)
                _handler?.UnequipItem(equippable.Slot);
        }
    }
}
