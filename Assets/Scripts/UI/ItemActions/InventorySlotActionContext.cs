using Core.Contracts.Equipment;
using Core.Contracts.Inventory;
using Core.Contracts.Shared;
using Data.Items;
using UnityEngine;

namespace UI.ItemActions
{
    public class InventorySlotActionContext : IActionableItem
    {
        private readonly IInventorySlot _slot;
        private readonly IItemActionHandler _handler;

        public InventorySlotActionContext(IInventorySlot slot, IItemActionHandler handler)
        {
            _slot = slot;
            _handler = handler;
        }

        public string DisplayName => _slot?.ItemData?.ItemName ?? string.Empty;
        public bool CanUse => _slot?.ItemData is ConsumableItem;
        public bool CanDrop => true;
        public bool CanEquip => _slot?.ItemData is IEquippable && !(_handler?.IsEquipped(_slot) ?? false);
        public bool CanUnequip => _slot?.ItemData is IEquippable && (_handler?.IsEquipped(_slot) ?? false);

        public void Use()
        {
            Debug.Log($"[InventorySlotActionContext] Use() called. Forwarding to _handler.UseItem() for item: {DisplayName}");
            _handler?.UseItem(_slot);
        }

        public void Drop()
        {
            Debug.Log($"[InventorySlotActionContext] Drop() called. Forwarding to _handler.DropItem() for item: {DisplayName}");
            _handler?.DropItem(_slot);
        }

        public void Equip()
        {
            Debug.Log($"[InventorySlotActionContext] Equip() called. Handler is null: {_handler == null}");
            Debug.Log($"[InventorySlotActionContext] Equip() called. Forwarding to _handler.EquipItem() for item: {DisplayName}");
            _handler?.EquipItem(_slot);
        }

        public void Unequip()
        {
            if (_slot?.ItemData is IEquippable equippable)
            {
                Debug.Log($"[InventorySlotActionContext] Unequip() called. Forwarding to _handler.UnequipItem() for item: {DisplayName} in slot {equippable.Slot}");
                _handler?.UnequipItem(equippable.Slot);
            }
            else
            {
                Debug.LogWarning($"[InventorySlotActionContext] Unequip() called, but item {DisplayName} is not IEquippable.");
            }
        }
    }
}
