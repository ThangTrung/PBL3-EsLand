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
    public class CraftingIngredientSlotUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI amountText;

        public void Setup(CraftingIngredient ingredient, int currentAmount)
        {
            if (ingredient.Item == null) return;

            // Gán Icon
            if (icon != null)
            {
                icon.sprite = ingredient.Item.Icon;
            }

            // Gán Text định dạng màu sắc
            if (amountText != null)
            {
                int needAmount = ingredient.Amount;
                string colorHex = currentAmount >= needAmount ? "#2ECC71" : "#E74C3C";
                amountText.text = $"{ingredient.Item.ItemName}: <color={colorHex}>{currentAmount}</color>/{needAmount}";
            }
        }
    }
}
