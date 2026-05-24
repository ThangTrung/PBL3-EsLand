using System.Collections.Generic;
using Data.Crafting;
using UnityEngine;

namespace UI.Crafting
{
    /// <summary>
    /// Quản lý tổng của Canvas Crafting.
    /// Chịu trách nhiệm Bật/Tắt và khởi tạo danh sách RecipeSlotUI.
    /// </summary>
    public class CraftingPanelUI : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private List<CraftingRecipe> availableRecipes;
        [SerializeField] private KeyCode toggleKey = KeyCode.B;

        [Header("UI References")]
        [SerializeField] private GameObject panelRoot; // Bảng to chứa cả 2 bảng trái phải
        [SerializeField] private Transform recipeListContainer; // Bảng trái (Banner To)
        [SerializeField] private RecipeSlotUI recipeSlotPrefab;
        [SerializeField] private CraftingDetailUI detailUI; // Bảng phải

        private void Start()
        {
            InitializeRecipes();
            
            // Ẩn panel lúc mới vào game
            if (panelRoot != null) panelRoot.SetActive(false);
            if (detailUI != null) detailUI.UpdateDetails(null); // Ẩn chi tiết
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                TogglePanel();
            }
        }

        private void TogglePanel()
        {
            if (panelRoot == null) return;
            
            bool isActive = !panelRoot.activeSelf;
            panelRoot.SetActive(isActive);
            
            // Khi mở lên, nếu đang có một công thức được chọn trước đó thì nên refresh lại UI
            // để lỡ người chơi vừa nhặt thêm rác thì UI nguyên liệu cập nhật luôn
            if (isActive && detailUI != null && detailUI.gameObject.activeSelf)
            {
                // Mẹo nhỏ: Gọi một hàm refresh nếu cần. Ở đây để đơn giản ta có thể yêu cầu click lại.
            }
        }

        private void InitializeRecipes()
        {
            if (recipeSlotPrefab == null || recipeListContainer == null) return;

            foreach (var recipe in availableRecipes)
            {
                if (recipe == null) continue;

                // Sinh ra Slot mới
                RecipeSlotUI slot = Instantiate(recipeSlotPrefab, recipeListContainer);
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
