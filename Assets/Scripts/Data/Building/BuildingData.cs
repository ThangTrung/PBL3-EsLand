using UnityEngine;

namespace Data.Building
{
    public enum BuildingType
    {
        RespawnPoint,   // Nhà hồi sinh (House1)
        Processing,     // Nhà chế biến (Tower)
        EscapeVehicle   // Thuyền thoát hiểm (Boat)
    }

    /// <summary>
    /// ScriptableObject định nghĩa các thuộc tính riêng của một công trình SAU KHI nó đã được xây dựng.
    /// Không chứa công thức chế tạo (Vì CraftingRecipe đã đảm nhận việc đó).
    /// </summary>
    [CreateAssetMenu(fileName = "New Building Data", menuName = "Building/Building Data")]
    public class BuildingData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "";

        [Header("Building Attributes")]
        [SerializeField] private string buildingName = "New Building";
        [SerializeField] [TextArea] private string description = "Mô tả công trình...";
        [SerializeField] private Sprite icon;
        [SerializeField] private BuildingType type;

        [Header("Stats")]
        [Tooltip("Lượng máu tối đa của công trình (nếu có thể bị quái phá)")]
        [SerializeField] private int maxHealth = 100;
        
        [Tooltip("Tầm hoạt động/Tương tác (Ví dụ: bán kính sáng của lửa, bán kính nhận diện của tháp)")]
        [SerializeField] private float interactRadius = 3f;

        [Header("Visuals & Instantiation")]
        [Tooltip("Prefab hiển thị của công trình trong Scene")]
        [SerializeField] private GameObject buildingPrefab;

        // Properties (Tính đóng gói)
        public string ID => string.IsNullOrEmpty(id) ? name : id;
        public string BuildingName => buildingName;
        public string Description => description;
        public Sprite Icon => icon;
        public BuildingType Type => type;
        public int MaxHealth => maxHealth;
        public float InteractRadius => interactRadius;
        public GameObject BuildingPrefab => buildingPrefab;

        private void OnValidate()
        {
            // Tự động đồng bộ ID với tên file Asset để chuẩn hóa
            if (id == name) return;
            id = name;
                
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
