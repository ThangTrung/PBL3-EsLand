using System;
using Data.Items;
using UnityEngine;

namespace Data.Crafting
{
    /// <summary>
    /// Đại diện cho một nguyên liệu cần thiết trong quá trình chế tạo.
    /// Sử dụng struct để tối ưu bộ nhớ và tránh null reference cho các thuộc tính dữ liệu.
    /// </summary>
    [Serializable]
    public struct CraftingIngredient
    {
        [Tooltip("Vật phẩm yêu cầu")]
        [SerializeField] private ItemData item;

        [Tooltip("Số lượng yêu cầu")]
        [SerializeField] [Min(1)] private int amount;

        public ItemData Item => item;
        public int Amount => amount;

        public CraftingIngredient(ItemData item, int amount)
        {
            this.item = item;
            this.amount = Mathf.Max(1, amount);
        }
    }
}
