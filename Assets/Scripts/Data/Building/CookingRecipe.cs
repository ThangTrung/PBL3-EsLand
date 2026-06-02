using Data.Items;
using UnityEngine;

namespace Data.Building
{
    /// <summary>
    /// Lưu trữ dữ liệu cấu hình cho một Công thức nung/nấu.
    /// Đã được refactor để đồng bộ với kiến trúc ItemData và CraftingRecipe (có trường ID, tự động gán tên).
    /// </summary>
    [CreateAssetMenu(fileName = "New Cooking Recipe", menuName = "Building/Cooking Recipe")]
    public class CookingRecipe : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "";

        [Header("Recipe Settings")]
        [SerializeField] private ItemData inputItem;
        [SerializeField] private ItemData outputItem;
        [SerializeField] private float cookingTime = 5f;

        // Properties
        public string ID => id;
        public ItemData InputItem => inputItem;
        public ItemData OutputItem => outputItem;
        public float CookingTime => cookingTime;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = System.Guid.NewGuid().ToString();
            }
                
#if UNITY_EDITOR
            // Đánh dấu asset đã thay đổi để Unity lưu lại
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
