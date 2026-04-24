using Script.Interfaces;
using UnityEngine;

namespace Script.Inventory.Controller
{
    public class InventoryActionHandler : IItemActionHandler
    {
        private readonly IInventory _inventory;
        private readonly Entities.Character _owner;

        public InventoryActionHandler(IInventory inventory, Entities.Character owner)
        {
            _inventory = inventory;
            _owner = owner;
        }

        public void UseItem(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty || _owner == null) return;
            if (slot.Item is IItemUsable usable && usable.Use(_owner))
            {
                _inventory.ConsumeSlot(slot, 1);
            }
        }

        public void DropItem(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty) return;
            slot.Item.Drop();
            _inventory.RemoveSlot(slot);
        }

        public void EquipItem(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty || _owner == null) return;
            if (slot.Item is IEquippable equippable)
            {
                // If it's a Tool, we might want to check for durability
                if (slot.Item is IDurable durable && slot.CurrentDurability <= 0)
                {
                    Debug.LogWarning("Vật phẩm đã hỏng, không thể trang bị!");
                    return;
                }
                
                _owner.Equip(equippable);
                _inventory.NotifyChanged(); // Trigger UI refresh
            }
        }

        public void UnequipItem(Items.EquipSlot slot)
        {
            if (_owner == null) return;
            _owner.Unequip(slot);
            _inventory.NotifyChanged();
        }

        public bool IsEquipped(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty || !_owner) return false;
            if (slot.Item is not IEquippable equippable) return false;
            
            // This is a bit simplified, ideally we check against a real equipment manager
            // For now, we assume if it's equippable, we can check its slot
            var manager = _owner.GetComponent<Entities.EquipmentManager>();
            if (!manager) return false;
            
            var equipped = manager.GetEquippedItem(equippable.EquipSlot);
            return equipped != null && ReferenceEquals(equipped, slot.Item);
        }
    }
}
