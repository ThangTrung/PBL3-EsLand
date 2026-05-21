using Core.Contracts.Equipment;
using Core.Contracts.Inventory;
using Core.Contracts.Shared;
using Data.Equipment;using Data.Items;

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
            _ownerFacade = GetComponentInParent<Character>();
        }

        public void UseItem(IInventorySlot slot)
        {
            Debug.Log($"Using item: {slot.ItemData?.ItemName ?? "null"}");
            if (slot.IsEmpty || _ownerFacade == null) return;
            if (slot.ItemData is IItemUsable usable && usable.Use(_ownerFacade))
                _inventory.ConsumeSlot(slot, 1);
        }

        public void DropItem(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty) return;
            _inventory.RemoveSlot(slot);
        }

        public void EquipItem(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty || _ownerFacade == null) return;
            if (slot.ItemData is not IEquippable equippable) return;
            
            if (_ownerFacade.EquipmentManager == null)
            {
                Debug.LogWarning($"Cannot equip item: {_ownerFacade.name} has no EquipmentManager component!");
                return;
            }

            if (slot.ItemData is IDurable && slot.CurrentDurability <= 0)
            {
                Debug.LogWarning("Item is broken, cannot equip!");
                return;
            }

            var oldItem = _ownerFacade.EquipmentManager.GetEquippedItem(equippable.Slot);
            _inventory.RemoveSlot(slot);
            _ownerFacade.EquipmentManager.Equip(equippable);

            if (oldItem is ItemData oldItemData)
            {
                _inventory.AddItem(oldItemData);
            }

            _inventory.NotifyChanged();
        }

        public void UnequipItem(EquipSlot slot)
        {
            if (_ownerFacade == null || _ownerFacade.EquipmentManager == null) return;
            
            var item = _ownerFacade.EquipmentManager.GetEquippedItem(slot);
            if (item == null) return;

            if (item is ItemData itemData)
            {
                if (_inventory.AddItem(itemData))
                {
                    _ownerFacade.EquipmentManager.Unequip(slot);
                }
                else
                {
                    Debug.LogWarning("Inventory full, cannot unequip!");
                }
            }
                
            _inventory.NotifyChanged();
        }

        public void DropEquippedItem(EquipSlot slot)
        {
            if (_ownerFacade != null && _ownerFacade.EquipmentManager != null)
                _ownerFacade.EquipmentManager.Unequip(slot);
                
            _inventory.NotifyChanged();
        }


        public bool IsEquipped(IInventorySlot slot)
        {
            if (slot == null || slot.IsEmpty || _ownerFacade == null) return false;
            if (slot.ItemData is not IEquippable equippable) return false;
            
            var equipped = _ownerFacade.EquipmentManager?.GetEquippedItem(equippable.Slot);
            return equipped != null && ReferenceEquals(equipped, slot.ItemData);
        }
    }
}
