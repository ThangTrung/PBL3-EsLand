using Core.Contracts.Inventory;
using Core.Types;
using Data.Crafting;
using Data.Equipment;

namespace Gameplay.Crafting
{
    /// <summary>
    /// Service chuyên xử lý logic chế tạo vật phẩm.
    /// Tách biệt hoàn toàn khỏi Inventory và UI, tuân thủ nguyên tắc SRP.
    /// </summary>
    public static class CraftingService
    {
        /// <summary>
        /// Kiểm tra xem người chơi có đủ điều kiện (nguyên liệu và công cụ) để chế tạo không.
        /// </summary>
        public static bool CanCraft(CraftingRecipe recipe, IInventoryHolder holder)
        {
            if (recipe == null || holder == null || holder.Inventory == null) return false;

            // 1. Kiểm tra công cụ yêu cầu (ví dụ: cần Búa để xây nhà)
            if (recipe.RequiredTool != ToolType.None)
            {
                if (holder.EquipmentManager == null) return false;

                var equippedItem = holder.EquipmentManager.GetEquippedItem(EquipSlot.MainHand);
                // Kiểm tra xem món đồ đang cầm có phải là Tool và có đúng loại yêu cầu không
                if (equippedItem is not Tool tool || tool.Type != recipe.RequiredTool)
                {
                    return false;
                }
            }

            // 2. Kiểm tra nguyên liệu trong túi đồ
            foreach (var ingredient in recipe.Ingredients)
            {
                if (holder.Inventory.CountItem(ingredient.Item) < ingredient.Amount)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Thực hiện quá trình chế tạo: Trừ nguyên liệu và thêm thành phẩm vào túi đồ.
        /// </summary>
        public static bool TryCraft(CraftingRecipe recipe, IInventoryHolder holder)
        {
            if (!CanCraft(recipe, holder))
            {
                return false;
            }

            // 1. Trừ nguyên liệu
            foreach (var ingredient in recipe.Ingredients)
            {
                holder.Inventory.RemoveItem(ingredient.Item, ingredient.Amount);
            }

            // 2. Thêm thành phẩm vào túi đồ
            bool added = holder.Inventory.AddItem(recipe.ResultItem, 1);
            
            return added;
        }
    }
}
