using System;
using System.Collections.Generic;
using Core.Contracts.Inventory;
using Data.Crafting;
using UnityEngine;

namespace UI.Crafting
{
    /// <summary>
    /// Quản lý tổng của Canvas Crafting.
    /// Chịu trách nhiệm Bật/Tắt và khởi tạo danh sách CraftingRecipeSlotUI.
    /// </summary>
    public class CraftingPanelUI : MonoBehaviour
    {
        public event Action OnCraftingClosed;

        [Header("Configuration")]
        [SerializeField] private List<CraftingRecipe> availableRecipes;

        [Header("UI References")]
        [SerializeField] private GameObject panelRoot; // Bảng to chứa cả 2 bảng trái phải
        [SerializeField] private Transform recipeListContainer; // Bảng trái (Banner To)
        [SerializeField] private CraftingRecipeSlotUI recipeSlotPrefab;
        [SerializeField] private CraftingDetailUI detailUI; // Bảng phải

        public bool IsVisible => panelRoot != null && panelRoot.activeSelf;

        private IInventoryHolder _inventoryHolder;

        public void Initialize(IInventoryHolder inventoryHolder)
        {
            _inventoryHolder = inventoryHolder;
            if (detailUI != null)
            {
                detailUI.Initialize(inventoryHolder);
            }
            
            InitializeRecipes();
            
            // Ẩn panel lúc mới vào game
            if (panelRoot != null) panelRoot.SetActive(false);
            if (detailUI != null) detailUI.UpdateDetails(null); // Ẩn chi tiết
        }

        public void ToggleUI()
        {
            SetVisible(!IsVisible);
        }

        public void SetVisible(bool visible)
        {
            if (panelRoot == null) return;
            
            panelRoot.SetActive(visible);

            if (_inventoryHolder is Gameplay.Characters.Player player)
            {
                player.SetCraftingState(visible);
            }
            
            if (visible)
            {
                // Khi mở lên, cập nhật lại chi tiết nếu đang chọn công thức
                if (detailUI != null)
                {
                    detailUI.Refresh();
                }
            }
            else
            {
                OnCraftingClosed?.Invoke();
            }
        }

        private void InitializeRecipes()
        {
            if (recipeSlotPrefab == null || recipeListContainer == null) return;

            // Xóa các slot cũ nếu có
            foreach (Transform child in recipeListContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var recipe in availableRecipes)
            {
                if (recipe == null) continue;

                // Sinh ra Slot mới
                CraftingRecipeSlotUI slot = Instantiate(recipeSlotPrefab, recipeListContainer);
                slot.Init(recipe);
                
                // Đăng ký sự kiện: Khi slot này bị click, truyền dữ liệu qua DetailUI
                slot.OnRecipeSelected += HandleRecipeSelected;
            }
        }

        private void HandleRecipeSelected(CraftingRecipe recipe)
        {
            if (detailUI != null)
            {
                detailUI.UpdateDetails(recipe);
            }
        }
    }
}
