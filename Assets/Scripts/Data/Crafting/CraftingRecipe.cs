using System.Collections.Generic;
using Core.Types;
using Data.Items;
using UnityEngine;

namespace Data.Crafting
{
    /// <summary>
    /// Lưu trữ dữ liệu cấu hình cho một Công thức chế tạo.
    /// Tách biệt dữ liệu định nghĩa khỏi Logic xử lý (Crafting System).
    /// </summary>
    [CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Crafting/Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        [Header("Recipe Info")]
        [SerializeField] private string id = "";
        [SerializeField] private string recipeName = "New Recipe";
        [SerializeField] [TextArea] private string description = "Mô tả công thức chế tạo...";

        [Header("Requirements")]
        [SerializeField] private ToolType requiredTool = ToolType.None;
        [SerializeField] private List<CraftingIngredient> ingredients = new List<CraftingIngredient>();

        [Header("Results")]
        [SerializeField] private ItemData resultItem;
        [Tooltip("Prefab vật lý để spawn ra thế giới khi chế tạo thành công")]
        [SerializeField] private GameObject resultPrefab;

        public string ID => id;
        public string RecipeName => recipeName;
        public string Description => description;
        
        // Sử dụng IReadOnlyList để bảo vệ tính toàn vẹn của dữ liệu, ngăn việc sửa đổi từ bên ngoài (Encapsulation)
        public IReadOnlyList<CraftingIngredient> Ingredients => ingredients;
        public ToolType RequiredTool => requiredTool;
        
        public ItemData ResultItem => resultItem;
        public GameObject ResultPrefab => resultPrefab;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = System.Guid.NewGuid().ToString();
            }
                
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
