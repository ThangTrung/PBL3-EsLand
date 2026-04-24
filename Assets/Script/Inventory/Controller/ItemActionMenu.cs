using Script.Interfaces;
using UnityEngine;
using Script.Inventory.UI;

namespace Script.Inventory.Controller
{
    public class ItemActionMenu : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ItemActionMenuUI menuUI;
        [SerializeField] private MonoBehaviour inventoryProvider;
        private IInventory Inventory => inventoryProvider as IInventory;

        private IInventorySlot _currentSlot;
        
        private void OnEnable()
        {
            if (menuUI == null) return;
            menuUI.OnUseClicked += HandleUse;
            menuUI.OnDropClicked += HandleDrop;
            menuUI.OnEquipClicked += HandleEquip;
            menuUI.OnUnequipClicked += HandleUnequip;
        }

        private void OnDisable()
        {
            if (menuUI == null) return;
            menuUI.OnUseClicked -= HandleUse;
            menuUI.OnDropClicked -= HandleDrop;
            menuUI.OnEquipClicked -= HandleEquip;
            menuUI.OnUnequipClicked -= HandleUnequip;
        }

        public void ShowMenu(IInventorySlot slotData, Vector3 worldPos)
        {
            _currentSlot = slotData;
            if (_currentSlot == null || _currentSlot.IsEmpty) return;

            var canUse = _currentSlot.Item is IItemUsable;
            var isEquippable = _currentSlot.Item is IEquippable equippable;
            var isEquipped = false;
            
            if (isEquippable && Inventory?.ActionHandler != null)
            {
                isEquipped = Inventory.ActionHandler.IsEquipped(_currentSlot);
            }
            
            // Show Equip if not equipped, show Unequip if already equipped
            menuUI.Show(worldPos, canUse, isEquippable && !isEquipped, isEquippable && isEquipped);
        }

        public void HideMenu()
        {
            _currentSlot = null;
            if (menuUI) menuUI.Hide();
        }

        private void HandleUse()
        {
            if (_currentSlot != null && Inventory?.ActionHandler != null)
            {
                Inventory.ActionHandler.UseItem(_currentSlot);
            }
            HideMenu();
        }

        private void HandleEquip()
        {
            if (_currentSlot != null && Inventory?.ActionHandler != null)
            {
                Inventory.ActionHandler.EquipItem(_currentSlot);
            }
            HideMenu();
        }

        private void HandleUnequip()
        {
            if (_currentSlot != null && _currentSlot.Item is IEquippable equippable && Inventory?.ActionHandler != null)
            {
                Inventory.ActionHandler.UnequipItem(equippable.EquipSlot);
            }
            HideMenu();
        }

        private void HandleDrop()
        {
            if (_currentSlot != null && Inventory?.ActionHandler != null)
            {
                Inventory.ActionHandler.DropItem(_currentSlot);
            }
            HideMenu();
        }
    }
}