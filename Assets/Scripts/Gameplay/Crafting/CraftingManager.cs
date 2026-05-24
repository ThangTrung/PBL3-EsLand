using Data.Crafting;
using Gameplay.Inventory;
using UnityEngine;

namespace Gameplay.Crafting
{
    /// <summary>
    /// Quản lý logic chế tạo vật phẩm.
    /// Tương tác trực tiếp với dữ liệu công thức (CraftingRecipe) và túi đồ (InventoryController).
    /// Hoàn toàn độc lập với UI.
    /// </summary>
    public class CraftingManager : MonoBehaviour
    {
        /// <summary>
        /// Kiểm tra xem túi đồ của người chơi có đủ tất cả nguyên liệu để chế tạo công thức này không.
        /// </summary>
        public bool CanCraft(CraftingRecipe recipe, InventoryController playerInventory)
        {
            if (recipe == null || playerInventory == null) return false;

            foreach (var ingredient in recipe.Ingredients)
            {
                // Sử dụng CountItem của InventoryController để đếm tổng số lượng vật phẩm (dựa trên ID)
                if (playerInventory.CountItem(ingredient.Item) < ingredient.Amount)
                {
                    return false; // Chỉ cần thiếu 1 nguyên liệu là trả về false ngay
                }
            }

            return true; // Đã đủ TẤT CẢ nguyên liệu
        }

        /// <summary>
        /// Thực hiện quá trình chế tạo: Trừ nguyên liệu và spawn vật phẩm ra thế giới.
        /// </summary>
        public bool CraftItem(CraftingRecipe recipe, InventoryController playerInventory, Transform spawnLocation)
        {
            // 1. Double-check: Đảm bảo có thể craft trước khi thực sự trừ đồ
            if (!CanCraft(recipe, playerInventory))
            {
                Debug.LogWarning($"[CraftingManager] Không đủ nguyên liệu để chế tạo: {recipe.RecipeName}");
                return false;
            }

            if (spawnLocation == null)
            {
                Debug.LogError("[CraftingManager] Thiếu vị trí Spawn (spawnLocation)!");
                return false;
            }

            // 2. Trừ nguyên liệu
            foreach (var ingredient in recipe.Ingredients)
            {
                bool removed = playerInventory.RemoveItem(ingredient.Item, ingredient.Amount);
                if (!removed)
                {
                    // Lỗi nghiêm trọng: Hàm CanCraft đã check là đủ, nhưng lúc Remove lại lỗi (có thể do race condition hoặc lỗi logic RemoveItem).
                    Debug.LogError($"[CraftingManager] Lỗi khi trừ nguyên liệu: {ingredient.Item.ItemName}");
                    return false; 
                }
            }

            // 3. Sinh ra sản phẩm (Spawn)
            if (recipe.ResultPrefab != null)
            {
                Instantiate(recipe.ResultPrefab, spawnLocation.position, Quaternion.identity);
                Debug.Log($"<color=green>[CraftingManager] Chế tạo thành công:</color> {recipe.RecipeName}. Sản phẩm đã rơi ra đất.");
            }
            else
            {
                Debug.LogWarning($"[CraftingManager] Chế tạo thành công nhưng ResultPrefab bị trống: {recipe.RecipeName}");
            }

            return true;
        }
    }
}
