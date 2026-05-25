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
    public class IngredientSlotUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI amountText;

        public void Setup(CraftingIngredient ingredient, int currentAmount)
        {
            if (ingredient.Item == null) return;

            // Tính toán lại currentAmount thực tế từ InventoryController
            currentAmount = FindObjectOfType<Gameplay.Inventory.InventoryController>().CountItem(ingredient.Item);

            // Gán Icon
            if (icon != null)
            {
                icon.sprite = ingredient.Item.Icon;
            }

            // Gán Text định dạng màu sắc
            if (amountText != null)
            {
                int needAmount = ingredient.Amount;
                string colorHex = currentAmount >= needAmount ? "black" : "red";
                amountText.text = $"{ingredient.Item.ItemName}: <color={colorHex}>{currentAmount}</color>/{needAmount}";
            }
        }
    }
}
