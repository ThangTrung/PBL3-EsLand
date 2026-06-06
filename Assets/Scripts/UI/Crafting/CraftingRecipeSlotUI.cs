using System;
using Data.Crafting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.Crafting
{
    /// <summary>
    /// Gắn lên từng Button (Ô công thức) ở Bảng Trái.
    /// Hierarchy: Button (RecipeSlot) -> Image (Icon), Image (Highlight)
    /// </summary>
    public class CraftingRecipeSlotUI : SlotUIBase
    {
        private CraftingRecipe currentRecipe;
        private Button button;

        // Event (Action) để báo cho Panel biết công thức này vừa được click (Loose Coupling)
        public event Action<CraftingRecipe> OnRecipeSelected;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);
        }

        public void Init(CraftingRecipe recipe)
        {
            currentRecipe = recipe;
            if (iconImage != null && recipe != null && recipe.ResultItem != null)
            {
                _hasData = true;
                _cachedTitle = recipe.ResultItem.ItemName;
                _cachedContent = recipe.ResultItem.Description;
                
                iconImage.sprite = recipe.ResultItem.Icon;
                iconImage.enabled = true; // Đảm bảo icon luôn hiển thị khi có dữ liệu
            }
            else
            {
                ClearVisuals();
            }

            // Mặc định tắt viền highlight khi mới khởi tạo
            SetHighlight(false);
        }

        private void HandleClick()
        {
            if (currentRecipe != null)
            {
                OnRecipeSelected?.Invoke(currentRecipe);
            }
        }
    }
}
