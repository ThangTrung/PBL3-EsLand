using Core.Contracts.Equipment;
using Core.Contracts.Inventory;
using Core.Contracts.Shared;
using Data.Equipment;
using Gameplay.Characters;
using UnityEngine;

namespace Gameplay.Inventory
{
    [RequireComponent(typeof(InventoryController))]
    public class InventoryActionService : MonoBehaviour, IItemActionHandler
    {
        private InventoryController _inventory;
        private Character _ownerFacade;

        private void Awake()
        {
            _inventory = GetComponent<InventoryController>();
            _ownerFacade = GetComponent<Character>();
        }

        public void UseItem(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty || _ownerFacade == null) return;
            if (slot.Item is IItemUsable usable && usable.Use(_ownerFacade))
                _inventory.ConsumeSlot(slot, 1);
        }

        public void DropItem(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty) return;
            // The item data should handle spawning a drop in the world
            // slot.Item.Drop(); // Depending on implementation
            _inventory.RemoveSlot(slot);
        }

        public void EquipItem(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty || _ownerFacade == null) return;
            if (slot.Item is not IEquippable equippable) return;
            if (slot.Item is IDurable durable && slot.CurrentDurability <= 0)
            {
                Debug.LogWarning("Item is broken, cannot equip!");
                return;
            }
            if (_ownerFacade.EquipmentManager != null)
                _ownerFacade.EquipmentManager.Equip(equippable);
                
            _inventory.NotifyChanged();
        }

        public void UnequipItem(EquipSlot slot)
        {
            if (_ownerFacade != null && _ownerFacade.EquipmentManager != null)
                _ownerFacade.EquipmentManager.Unequip(slot);
                
            _inventory.NotifyChanged();
        }

        public bool IsEquipped(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty || _ownerFacade == null) return false;
            if (slot.Item is not IEquippable equippable) return false;
            
            var equipped = _ownerFacade.EquipmentManager?.GetEquippedItem(equippable.Slot);
            return equipped != null && ReferenceEquals(equipped, slot.Item);
        }
    }
}
