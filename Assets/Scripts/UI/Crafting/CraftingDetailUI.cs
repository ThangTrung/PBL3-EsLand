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
        private InventoryController playerInventory;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI recipeNameText;
        [SerializeField] private Transform ingredientsContainer;
        
        [Tooltip("Prefab chứa IngredientSlotUI (có Icon và Text)")]
        [SerializeField] private IngredientSlotUI ingredientSlotPrefab; 
        
        [SerializeField] private Button craftButton;

        private CraftingRecipe currentRecipe;

        private void Awake()
        {
            if (craftButton != null)
            {
                craftButton.onClick.AddListener(OnCraftButtonClicked);
            }
        }

        private void Start()
        {
            if (playerInventory == null)
            {
                playerInventory = FindObjectOfType<Gameplay.Inventory.InventoryController>();
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
                int have = playerInventory != null ? playerInventory.CountItem(ingredient.Item) : 0;
                if (have < ingredient.Amount) canCraft = false;

                IngredientSlotUI slotUI = Instantiate(ingredientSlotPrefab, ingredientsContainer);
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
            Debug.Log("1. Đã bấm nút chế tạo!");

            if (currentRecipe == null)
            {
                Debug.LogWarning("currentRecipe bị Null!");
                return;
            }
            if (playerInventory == null)
            {
                Debug.LogWarning("playerInventory bị Null!");
                return;
            }

            if (playerInventory.TryCraftRecipe(currentRecipe))
            {
                UpdateDetails(currentRecipe);
                Debug.Log("Chế tạo thành công!");
            }
            else
            {
                Debug.LogWarning("Không đủ nguyên liệu!");
            }
        }
    }
}
