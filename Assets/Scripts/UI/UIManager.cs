using Core.Contracts.Inventory;
using Core.Contracts.Shared;
using Core.Events;
using Gameplay.Characters;
using UI.Equipment;
using UI.Inventory;
using UI.ItemActions;
using UI.Status;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public InventoryPanelUI inventoryUI;
        public EquipmentPanelUI equipmentUI;
        public ItemActionMenu actionMenu;
        public StatusPanelUI statusUI;

        private void OnEnable()
        {
            GameEvents.OnPlayerReady += HandlePlayerReady;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerReady -= HandlePlayerReady;
            
            if (inventoryUI != null)
            {
                inventoryUI.OnActionMenuRequested -= OpenActionMenu;
                inventoryUI.OnInventoryClosed -= actionMenu.HideMenu;
            }

            if (equipmentUI != null)
            {
                equipmentUI.OnActionMenuRequested -= OpenActionMenu;
                equipmentUI.OnEquipmentClosed -= actionMenu.HideMenu;
            }
        }

        private void HandlePlayerReady(IInventoryHolder inventoryHolder)
        {
            if (inventoryUI != null)
            {
                inventoryUI.Initialize(inventoryHolder);
                inventoryUI.OnActionMenuRequested += OpenActionMenu;
                inventoryUI.OnInventoryClosed += actionMenu.HideMenu;
                Debug.Log("<color=green>[UIManager]</color> Đã khởi tạo và kết nối Inventory UI.");
            }

            if (equipmentUI != null)
            {
                equipmentUI.Initialize(inventoryHolder);
                equipmentUI.OnActionMenuRequested += OpenActionMenu;
                equipmentUI.OnEquipmentClosed += actionMenu.HideMenu;
                Debug.Log("<color=green>[UIManager]</color> Đã khởi tạo Equipment UI.");
            }

            if (inventoryHolder is not Player player) return;

            if (statusUI != null)
            {
                statusUI.Initialize(player);
                Debug.Log("<color=green>[UIManager]</color> Đã khởi tạo Status UI.");
            }

            if (inventoryUI != null)
            {
                player.OnToggleInventory -= inventoryUI.ToggleUI;
                player.OnToggleInventory += inventoryUI.ToggleUI;
            }

            if (equipmentUI == null) return;
            player.OnToggleEquipment -= equipmentUI.ToggleUI;
            player.OnToggleEquipment += equipmentUI.ToggleUI;
        }

        private void OpenActionMenu(IActionableItem context, Vector3 pos)
        {
            if (actionMenu)
            {
                actionMenu.ShowMenu(context, pos);
            }
        }
    }
}