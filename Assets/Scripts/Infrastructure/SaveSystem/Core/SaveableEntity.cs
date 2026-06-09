using UnityEngine;

namespace Infrastructure.SaveSystem.Core
{
    /// <summary>
    /// Gắn component này vào bất kỳ Object nào cần lưu trạng thái (Cây, Đá, Item rơi).
    /// Tự động sinh ID duy nhất để quản lý trong GameData.
    /// </summary>
    public class SaveableEntity : MonoBehaviour
    {
        [SerializeField] private string id = "";

        public string Id => id;

        private void Awake()
        {
            // [SENIOR FIX] Đảm bảo luôn có ID khi chạy, kể cả khi Instantiate từ code
            if (string.IsNullOrEmpty(id))
            {
                GenerateGuid();
            }
        }

        [ContextMenu("Generate Guid for ID")]
        public void GenerateGuid() // Đổi thành public để Tool bên ngoài gọi được
        {
            id = System.Guid.NewGuid().ToString();
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
            #endif
        }

        /// <summary>
        /// Ép sinh ID mới. Dùng cho Object Pooling khi tái sử dụng object 
        /// nhưng muốn nó được coi là một thực thể mới hoàn toàn.
        /// </summary>
        public void ForceNewId()
        {
            GenerateGuid();
        }

        private void OnValidate()
        {
            #if UNITY_EDITOR
            // Nếu là file Prefab gốc trong thư mục -> Ép ID trống để không lây nhiễm ID lỗi
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                id = "";
                return;
            }

            // Chỉ sinh mã khi ô này trống hoàn toàn (Né được việc quét toàn Scene gây lag)
            if (string.IsNullOrEmpty(id))
            {
                GenerateGuid();
            }
            #endif
        }
        
        // Helper để các class khác lấy ID nhanh (Giữ nguyên của Tiến)
        public static string GetEntityId(GameObject obj)
        {
            if (obj.TryGetComponent<SaveableEntity>(out var entity))
            {
                return entity.Id;
            }
            return null;
        }
    }
}