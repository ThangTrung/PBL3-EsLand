using Core.Contracts.Inventory;
using Core.Contracts.Shared;
using Core.Events;
using Data.Items;
using Gameplay.Characters;
using UI.Equipment;
using UI.Inventory;
using UI.ItemActions;
using UI.Status;
using UI.Building;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public InventoryPanelUI inventoryUI;
        public EquipmentPanelUI equipmentUI;
        public ItemActionMenu actionMenu;
        public StatusPanelUI statusUI;
        public CookingTowerUI cookingUI;

        private IInventory _playerInventory;

        private void OnEnable()
        {
            GameEvents.OnPlayerReady += HandlePlayerReady;
            Gameplay.Building.CookingTower.OnTowerInteracted += HandleTowerInteracted;

            if (cookingUI != null)
            {
                cookingUI.OnSlotClicked += HandleCookingSlotClicked;
            }
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerReady -= HandlePlayerReady;
            Gameplay.Building.CookingTower.OnTowerInteracted -= HandleTowerInteracted;
            
            if (inventoryUI != null)
            {
                inventoryUI.OnActionMenuRequested -= OpenActionMenu;
                inventoryUI.OnInventoryClosed -= actionMenu.HideMenu;
                inventoryUI.OnSlotLeftClicked -= HandleInventorySlotLeftClicked;
            }

            if (equipmentUI != null)
            {
                equipmentUI.OnActionMenuRequested -= OpenActionMenu;
                equipmentUI.OnEquipmentClosed -= actionMenu.HideMenu;
            }

            if (cookingUI != null)
            {
                cookingUI.OnSlotClicked -= HandleCookingSlotClicked;
            }
        }

        private void HandlePlayerReady(IInventoryHolder inventoryHolder)
        {
            if (inventoryHolder != null)
            {
                _playerInventory = inventoryHolder.Inventory;
            }

            if (inventoryUI != null)
            {
                inventoryUI.Initialize(inventoryHolder);
                inventoryUI.OnActionMenuRequested += OpenActionMenu;
                inventoryUI.OnInventoryClosed += actionMenu.HideMenu;
                inventoryUI.OnSlotLeftClicked += HandleInventorySlotLeftClicked;
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

        private void HandleTowerInteracted(Gameplay.Building.CookingTower tower)
        {
            if (inventoryUI != null && !inventoryUI.IsVisible)
            {
                inventoryUI.SetVisible(true);
            }
            if (cookingUI != null && !cookingUI.IsVisible)
            {
                cookingUI.OpenPanel();
            }
        }

        private void HandleInventorySlotLeftClicked(int index, IInventorySlot slot)
        {
            if (cookingUI == null || !cookingUI.IsVisible || cookingUI.CurrentTower == null) return;
            if (slot == null || slot.IsEmpty || _playerInventory == null) return;

            var item = slot.ItemData;
            bool added = false;

            // Áp dụng Logic Định Tuyến Nghiêm Ngặt (Strict Routing) dựa trên Type:
            // - ConsumableItem (nếu nấu được) -> BẮT BUỘC vào ô Input (Slot 0)
            // - MaterialItem (nếu là nhiên liệu) -> BẮT BUỘC vào ô Fuel (Slot 1)
            switch (item)
            {
                case ConsumableItem consumable when consumable.IsCookable:
                    added = cookingUI.CurrentTower.TryAddItem(0, item, 1);
                    break;

                case MaterialItem material when material.FuelTime > 0:
                    added = cookingUI.CurrentTower.TryAddItem(1, item, 1);
                    break;

                // Dễ dàng mở rộng trong tương lai, ví dụ:
                // case OreItem ore: added = cookingUI.CurrentTower.TryAddItem(0, item, 1); break;
            }

            if (added)
            {
                _playerInventory.ConsumeSlot(slot, 1);
            }
        }

        private void HandleCookingSlotClicked(int slotIndex, IInventorySlot slot)
        {
            if (cookingUI == null || !cookingUI.IsVisible || cookingUI.CurrentTower == null) return;
            if (slot == null || slot.IsEmpty || _playerInventory == null) return;

            // Rút toàn bộ số lượng trong slot của lò ra (hoặc rút từng cái, ở đây rút toàn bộ cho giống lấy thành phẩm)
            var item = cookingUI.CurrentTower.WithdrawItem(slotIndex, out int amount);
            if (item == null || amount <= 0) return;
            // Cố gắng thêm vào túi đồ
            var success = _playerInventory.AddItem(item, amount);
            if (success) return;
            cookingUI.CurrentTower.TryAddItem(slotIndex, item, amount);
            Debug.LogWarning("Túi đồ đã đầy, không thể lấy vật phẩm từ lò!");
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1)) return;
            if (!EventSystem.current || EventSystem.current.IsPointerOverGameObject()) return;
            if (inventoryUI && inventoryUI.IsVisible) inventoryUI.SetVisible(false);
            if (equipmentUI && equipmentUI.IsVisible) equipmentUI.SetVisible(false);
            if (cookingUI && cookingUI.IsVisible) cookingUI.ClosePanel();
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