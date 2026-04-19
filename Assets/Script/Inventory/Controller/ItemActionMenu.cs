using UnityEngine;
using Script.Inventory.UI;

namespace Script.Inventory.Controller
{
    public class ItemActionMenu : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ItemActionMenuUI menuUI;
        [SerializeField] private InventoryController inventoryController;

        private InventorySlot _currentSlot;
        
        private void OnEnable()
        {
            menuUI.OnUseClicked += HandleUse;
            menuUI.OnDropClicked += HandleDrop;
            menuUI.OnEquipClicked += HandleEquip;
            menuUI.OnUnequipClicked += HandleUnequip;
        }

        private void OnDisable()
        {
            menuUI.OnUseClicked -= HandleUse;
            menuUI.OnDropClicked -= HandleDrop;
            menuUI.OnEquipClicked -= HandleEquip;
            menuUI.OnUnequipClicked -= HandleUnequip;
        }

        public void ShowMenu(InventorySlot slotData, Vector3 worldPos)
        {
            Debug.Log("ShowMenu");
            _currentSlot = slotData;
            var isEquipment = slotData.IsEquipment;
            menuUI.Show(worldPos, !isEquipment, isEquipment, false);
        }

        public void HideMenu()
        {
            _currentSlot = null;
            if (menuUI) menuUI.Hide();
        }

        private void HandleUse()
        {
            // Logic dùng item ở đây
            HideMenu();
        }

        private void HandleEquip()
        {
            // Logic trang bị ở đây
            HideMenu();
        }

        private void HandleUnequip()
        {
            // Logic tháo trang bị ở đây
            HideMenu();
        }

        private void HandleDrop()
        {
            if (_currentSlot != null && !_currentSlot.IsEmpty && inventoryController != null)
            {
                inventoryController.RemoveSlot(_currentSlot);
                // Tạo prefab rơi ra đất tại đây
            }
            HideMenu();
        }
    }
}