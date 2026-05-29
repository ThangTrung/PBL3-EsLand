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
    [RequireComponent(typeof(Button))]
    public class CraftingRecipeSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image icon;
        [Tooltip("Hình ảnh viền/background sáng lên khi đưa chuột vào")]
        [SerializeField] private Image highlightImage;
        
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
            if (icon != null && recipe != null && recipe.ResultItem != null)
            {
                icon.sprite = recipe.ResultItem.Icon;
            }

            // Mặc định tắt viền highlight khi mới khởi tạo
            if (highlightImage != null)
            {
                highlightImage.enabled = false;
            }
        }

        private void HandleClick()
        {
            if (currentRecipe != null)
            {
                OnRecipeSelected?.Invoke(currentRecipe);
            }
        }

        // Bật viền khi đưa chuột vào
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (highlightImage != null)
            {
                highlightImage.enabled = true;
            }
        }

        // Tắt viền khi rút chuột ra
        public void OnPointerExit(PointerEventData eventData)
        {
            if (highlightImage != null)
            {
                highlightImage.enabled = false;
            }
        }
    }
}
