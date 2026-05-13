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
        }

        private void Start()
        {
            if (inventoryUI == null) return;
            inventoryUI.OnActionMenuRequested += OpenActionMenu;
            inventoryUI.OnInventoryClosed += actionMenu.HideMenu;

            // 2. Nhạc trưởng lắng nghe tiếng hét từ trang bị (Sau này mày viết thêm)
            // if (equipmentUI != null)
            // {
            //     equipmentUI.OnActionMenuRequested += OpenActionMenu;
            //     equipmentUI.OnEquipmentClosed += actionMenu.HideMenu;
            // }
        }

        private void HandlePlayerReady(IInventoryHolder inventoryHolder)
        {
            // a. Khởi tạo cho cả hai bảng UI
            if (inventoryUI != null)
            {
                inventoryUI.Initialize(inventoryHolder);
                Debug.Log("<color=green>[UIManager]</color> Đã khởi tạo Inventory UI.");
            }

            if (equipmentUI != null)
            {
                equipmentUI.Initialize(inventoryHolder);
                Debug.Log("<color=green>[UIManager]</color> Đã khởi tạo Equipment UI.");
            }

            // b. Đăng ký lắng nghe các sự kiện bật/tắt từ Player
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
            if (actionMenu != null)
            {
                actionMenu.ShowMenu(context, pos);
            }
        }
    }
}