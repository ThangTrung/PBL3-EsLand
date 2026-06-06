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
using UI.Crafting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private InventoryPanelUI inventoryUI;
        [SerializeField] private EquipmentPanelUI equipmentUI;
        [SerializeField] private CraftingPanelUI craftingUI;
        [SerializeField] private ItemActionMenu actionMenu;
        [SerializeField] private StatusPanelUI statusUI;
        [SerializeField] private CookingTowerUI cookingUI;
        [SerializeField] private VictoryPanelUI victoryUI;
        [SerializeField] private UI.Transition.CloudTransitionUI cloudTransitionUI;

        private IInventory _playerInventory;
        private Player _currentPlayer;

        private void OnEnable()
        {
            GameEvents.OnPlayerReady += HandlePlayerReady;
            GameEvents.OnSleepRequested += HandleSleepRequested;
            GameEvents.OnVictory += HandleVictory;
            Gameplay.Building.CookingTower.OnTowerInteracted += HandleTowerInteracted;

            if (cookingUI != null)
            {
                cookingUI.OnSlotClicked += HandleCookingSlotClicked;
            }
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerReady -= HandlePlayerReady;
            GameEvents.OnSleepRequested -= HandleSleepRequested;
            GameEvents.OnVictory -= HandleVictory;
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

            // Dọn dẹp sự kiện để tránh Memory Leak / Double Subscribe
            if (_currentPlayer != null)
            {
                if (inventoryUI != null) _currentPlayer.OnToggleInventory -= inventoryUI.ToggleUI;
                if (equipmentUI != null) _currentPlayer.OnToggleEquipment -= equipmentUI.ToggleUI;
                if (craftingUI != null) _currentPlayer.OnToggleCrafting -= craftingUI.ToggleUI;
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
            }

            if (equipmentUI != null)
            {
                equipmentUI.Initialize(inventoryHolder);
                equipmentUI.OnActionMenuRequested += OpenActionMenu;
                equipmentUI.OnEquipmentClosed += actionMenu.HideMenu;
            }

            if (craftingUI != null)
            {
                craftingUI.Initialize(inventoryHolder);
            }

            if (inventoryHolder is not Player player) return;
            
            _currentPlayer = player;

            if (statusUI != null)
            {
                statusUI.Initialize(player);
            }

            if (inventoryUI != null)
            {
                player.OnToggleInventory -= inventoryUI.ToggleUI;
                player.OnToggleInventory += inventoryUI.ToggleUI;
            }

            if (equipmentUI != null)
            {
                player.OnToggleEquipment -= equipmentUI.ToggleUI;
                player.OnToggleEquipment += equipmentUI.ToggleUI;
            }

            if (craftingUI != null)
            {
                player.OnToggleCrafting -= craftingUI.ToggleUI;
                player.OnToggleCrafting += craftingUI.ToggleUI;
            }
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

            if (cookingUI.CurrentTower.SmartAddItem(slot.ItemData, 1))
            {
                _playerInventory.ConsumeSlot(slot, 1);
            }
        }

        private void HandleCookingSlotClicked(int slotIndex, IInventorySlot slot)
        {
            if (cookingUI == null || !cookingUI.IsVisible || cookingUI.CurrentTower == null) return;
            if (slot == null || slot.IsEmpty || _playerInventory == null) return;

            var item = cookingUI.CurrentTower.WithdrawItem(slotIndex, out int amount);
            if (item == null || amount <= 0) return;
            
            var success = _playerInventory.AddItem(item, amount);
            if (success) return;
            
            cookingUI.CurrentTower.TryAddItem(slotIndex, item, amount);
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1)) return;
            if (!EventSystem.current || EventSystem.current.IsPointerOverGameObject()) return;
            if (inventoryUI && inventoryUI.IsVisible) inventoryUI.SetVisible(false);
            if (equipmentUI && equipmentUI.IsVisible) equipmentUI.SetVisible(false);
            if (craftingUI && craftingUI.IsVisible) craftingUI.SetVisible(false);
            if (cookingUI && cookingUI.IsVisible) cookingUI.ClosePanel();
        }

        private void OpenActionMenu(IActionableItem context, Vector3 pos)
        {
            if (actionMenu)
            {
                actionMenu.ShowMenu(context, pos);
            }
        }

        private void HandleVictory()
        {
            // Khi thắng cuộc, đóng tất cả các Panel UI khác đang mở để tập trung vào màn hình chiến thắng
            if (inventoryUI != null) inventoryUI.SetVisible(false);
            if (equipmentUI != null) equipmentUI.SetVisible(false);
            if (craftingUI != null) craftingUI.SetVisible(false);
            if (cookingUI != null) cookingUI.ClosePanel();
            if (actionMenu != null) actionMenu.HideMenu();
        }

        private void HandleSleepRequested(Gameplay.Building.HomeSavePoint house, Player player)
        {
            if (cloudTransitionUI == null)
            {
                cloudTransitionUI = FindObjectOfType<UI.Transition.CloudTransitionUI>();
            }

            if (cloudTransitionUI != null)
            {
                StartCoroutine(PerformSleepTransition(house, player));
            }
            else
            {
                house.ExecuteSleepLogic(player);
            }
        }

        private System.Collections.IEnumerator PerformSleepTransition(Gameplay.Building.HomeSavePoint house, Player player)
        {
            // 1. Khoá người chơi nhưng CHƯA dừng game (để mây bay vào mượt mà)
            player.SetCanMove(false);

            // 2. Mây bay vào (Fade Out)
            yield return cloudTransitionUI.FadeOut(1.5f, "Đang nghỉ ngơi...");

            // 3. KHI MÂY ĐÃ CHE KÍN: Bắt đầu tạm dừng game
            Time.timeScale = 0;

            // 4. Thực hiện logic Gameplay tại nhà
            house.ExecuteSleepLogic(player);
            yield return new WaitForSecondsRealtime(0.5f);

            // 5. TRƯỚC KHI MÂY BAY RA: Chạy lại game
            Time.timeScale = 1;

            // 6. Mây bay ra (Fade In)
            yield return cloudTransitionUI.FadeIn(1.5f);

            // 7. Hoàn tất: Mở khoá người chơi
            player.SetCanMove(true);
        }
    }
}
