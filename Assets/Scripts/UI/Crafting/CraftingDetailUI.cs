using Core.Contracts.Inventory;
using Data.Crafting;
using Gameplay.Crafting;
using Gameplay.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Crafting
{
    /// <summary>
    /// Gắn lên Bảng Phải (Chi tiết công thức).
    /// Xử lý việc hiển thị thông tin và tương tác với CraftingManager.
    /// </summary>
    public class CraftingDetailUI : MonoBehaviour
    {
        private IInventoryHolder _inventoryHolder;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI recipeNameText;
        [SerializeField] private Transform ingredientsContainer;
        
        [Tooltip("Prefab chứa CraftingIngredientSlotUI (có Icon và Text)")]
        [SerializeField] private CraftingIngredientSlotUI ingredientSlotPrefab; 
        
        [SerializeField] private Button craftButton;

        private CraftingRecipe currentRecipe;

        private void Awake()
        {
            if (craftButton != null)
            {
                craftButton.onClick.AddListener(OnCraftButtonClicked);
            }
        }

        public void Initialize(IInventoryHolder inventoryHolder)
        {
            _inventoryHolder = inventoryHolder;
        }

        public void Refresh()
        {
            if (currentRecipe != null)
            {
                UpdateDetails(currentRecipe);
            }
        }

        public void UpdateDetails(CraftingRecipe recipe)
        {
            currentRecipe = recipe;
            
            if (recipe == null)
            {
                // Nếu truyền null thì ẩn bảng chi tiết đi
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (recipeNameText) recipeNameText.text = recipe.RecipeName;

            // 1. Xóa các UI nguyên liệu cũ
            foreach (Transform child in ingredientsContainer)
            {
                Destroy(child.gameObject);
            }

            // 2. Sinh UI nguyên liệu mới (có Icon)
            bool canCraft = true;
            foreach (var ingredient in recipe.Ingredients)
            {
                int have = 0;
                if (_inventoryHolder != null && _inventoryHolder.Inventory != null)
                {
                    have = _inventoryHolder.Inventory.CountItem(ingredient.Item);
                }
                
                if (have < ingredient.Amount) canCraft = false;

                CraftingIngredientSlotUI slotUI = Instantiate(ingredientSlotPrefab, ingredientsContainer);
                if (slotUI != null)
                {
                    slotUI.Setup(ingredient, have);
                }
            }

            // 3. Cập nhật trạng thái Nút Tạo
            if (craftButton != null)
            {
                craftButton.interactable = canCraft;
            }
        }

        public void OnCraftButtonClicked()
        {
            if (currentRecipe == null)
            {
                Debug.LogWarning("currentRecipe bị Null!");
                return;
            }
            if (_inventoryHolder == null || _inventoryHolder.Inventory == null)
            {
                Debug.LogWarning("inventoryHolder hoặc Inventory bị Null!");
                return;
            }

            // Gọi tới dịch vụ trung gian chuyên biệt cho Crafting
            if (CraftingService.TryCraft(currentRecipe, _inventoryHolder.Inventory))
            {
                UpdateDetails(currentRecipe);
                Debug.Log("Chế tạo thành công!");
            }
            else
            {
                Debug.LogWarning("Không đủ nguyên liệu hoặc không có chỗ trống trong túi đồ!");
            }
        }
    }
}
