using Data.Items;
using UnityEngine;

namespace Data.Building
{
    /// <summary>
    /// Lưu trữ dữ liệu cấu hình cho một Công thức nung/nấu.
    /// Tách biệt dữ liệu khỏi logic hoạt động của Lò nung (CookingTower).
    /// </summary>
    [CreateAssetMenu(fileName = "New Cooking Recipe", menuName = "Building/Cooking Recipe")]
    public class CookingRecipe : ScriptableObject
    {
        [Header("Recipe Settings")]
        [SerializeField] private ItemData inputItem;
        [SerializeField] private ItemData outputItem;
        [SerializeField] private float cookingTime = 5f;

        public ItemData InputItem => inputItem;
        public ItemData OutputItem => outputItem;
        public float CookingTime => cookingTime;
    }
}
