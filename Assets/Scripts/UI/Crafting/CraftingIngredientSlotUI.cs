using Data.Crafting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Crafting
{
    /// <summary>
    /// Gắn lên prefab đại diện cho 1 dòng nguyên liệu yêu cầu ở Bảng Phải.
    /// Hierarchy: Panel -> Image (Icon) + TextMeshProUGUI (Amount)
    /// </summary>
    public class CraftingIngredientSlotUI : SlotUIBase
    {
        public void Setup(CraftingIngredient ingredient, int currentAmount)
        {
            if (ingredient.Item == null) 
            {
                ClearVisuals();
                return;
            }

            _hasData = true;
            _cachedTitle = ingredient.Item.ItemName;
            _cachedContent = ingredient.Item.Description;

            // Gán Icon
            if (iconImage != null)
            {
                iconImage.sprite = ingredient.Item.Icon;
                iconImage.enabled = true;
            }

            // Gán Text định dạng màu sắc
            if (amountText != null)
            {
                amountText.enabled = true;
                int needAmount = ingredient.Amount;
                string colorHex = currentAmount >= needAmount ? "#2ECC71" : "#E74C3C";
                amountText.text = $"{ingredient.Item.ItemName}: <color={colorHex}>{currentAmount}</color>/{needAmount}";
            }
        }
    }
}
