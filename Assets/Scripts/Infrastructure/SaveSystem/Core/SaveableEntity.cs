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

        [ContextMenu("Generate Guid for ID")]
        private void GenerateGuid()
        {
            id = System.Guid.NewGuid().ToString();
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }

        private void OnValidate()
        {
            // Tự động sinh ID nếu trống khi đặt vào Scene hoặc tạo Prefab
            if (string.IsNullOrEmpty(id))
            {
                GenerateGuid();
            }
        }
        
        // Helper để các class khác lấy ID nhanh
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
