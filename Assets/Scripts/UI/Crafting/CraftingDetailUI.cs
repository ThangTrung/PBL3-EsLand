using Core.Contracts.Inventory;
using Data.Crafting;
using Data.Items;
using Gameplay.Crafting;
using Gameplay.Inventory;
using Infrastructure.Pooling;
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
        [SerializeField] private RectTransform ingredientsContainer;
        
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

            ClearIngredients();
            PopulateIngredients(recipe);
        }

        private void ClearIngredients()
        {
            // 1. Thu hồi các UI nguyên liệu cũ về ObjectPool
            for (int i = ingredientsContainer.childCount - 1; i >= 0; i--)
            {
                var child = ingredientsContainer.GetChild(i);
                // Chỉ thu hồi những object đang active (tránh ném 1 object vào pool 2 lần)
                if (child.gameObject.activeSelf)
                {
                    ObjectPoolManager.Instance.ReturnToPool(child.gameObject);
                }
            }
        }

        private void PopulateIngredients(CraftingRecipe recipe)
        {
            // 2. Sinh UI nguyên liệu mới từ ObjectPool
            foreach (var ingredient in recipe.Ingredients)
            {
                int have = GetInventoryItemCount(ingredient.Item);
                
                if (ingredientSlotPrefab != null)
                {
                    // [FIX UI SHRINK] Truyền trực tiếp parent vào Get và ObjectPoolManager sẽ lo phần reset scale/parent an toàn.
                    GameObject slotGO = ObjectPoolManager.Instance.Get(ingredientSlotPrefab.gameObject, Vector3.zero, Quaternion.identity, ingredientsContainer);
                    slotGO.transform.localPosition = Vector3.zero; // Đảm bảo vị trí local chuẩn trong Layout Group
                    slotGO.transform.SetAsLastSibling(); // Xếp đúng thứ tự hiển thị từ trên xuống
                    
                    CraftingIngredientSlotUI slotUI = slotGO.GetComponent<CraftingIngredientSlotUI>();
                    if (slotUI != null)
                    {
                        slotUI.Setup(ingredient, have);
                    }
                }
            }

            // 3. Cập nhật trạng thái Nút Tạo (Bao gồm kiểm tra nguyên liệu và công cụ yêu cầu)
            if (craftButton != null)
            {
                craftButton.interactable = CraftingService.CanCraft(recipe, _inventoryHolder);
            }
        }

        private int GetInventoryItemCount(ItemData item)
        {
            if (_inventoryHolder != null && _inventoryHolder.Inventory != null)
            {
                return _inventoryHolder.Inventory.CountItem(item);
            }
            return 0;
        }

        public void OnCraftButtonClicked()
        {
            if (currentRecipe == null)
            {
                return;
            }
            if (_inventoryHolder == null || _inventoryHolder.Inventory == null)
            {
                return;
            }

            // Gọi tới dịch vụ trung gian chuyên biệt cho Crafting
            if (CraftingService.TryCraft(currentRecipe, _inventoryHolder))
            {
                UpdateDetails(currentRecipe);
            }
            else
            {
            }
        }
    }
}
