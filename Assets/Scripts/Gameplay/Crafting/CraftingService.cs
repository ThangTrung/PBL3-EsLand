using Core.Contracts.Inventory;
using Data.Crafting;

namespace Gameplay.Crafting
{
    /// <summary>
    /// Service chuyên xử lý logic chế tạo vật phẩm.
    /// Tách biệt hoàn toàn khỏi Inventory và UI, tuân thủ nguyên tắc SRP.
    /// </summary>
    public static class CraftingService
    {
        /// <summary>
        /// Kiểm tra xem túi đồ có đủ tất cả nguyên liệu để chế tạo công thức này không.
        /// </summary>
        public static bool CanCraft(CraftingRecipe recipe, IInventory inventory)
        {
            if (recipe == null || inventory == null) return false;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (inventory.CountItem(ingredient.Item) < ingredient.Amount)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Thực hiện quá trình chế tạo: Trừ nguyên liệu và thêm thành phẩm vào túi đồ.
        /// </summary>
        public static bool TryCraft(CraftingRecipe recipe, IInventory inventory)
        {
            if (!CanCraft(recipe, inventory))
            {
                return false;
            }

            // 1. Trừ nguyên liệu
            foreach (var ingredient in recipe.Ingredients)
            {
                inventory.RemoveItem(ingredient.Item, ingredient.Amount);
            }

            // 2. Thêm thành phẩm vào túi đồ
            bool added = inventory.AddItem(recipe.ResultItem, 1);
            
            // Lưu ý: Nếu túi đồ đầy ngay lúc này, hàm AddItem có thể trả về false.
            // Trong thực tế, việc loại bỏ nguyên liệu thường sẽ tạo ra ô trống, 
            // nhưng nếu cần thiết có thể xử lý rớt đồ ra ngoài đất (Drop) ở đây nếu added == false.

            return added;
        }
    }
}
