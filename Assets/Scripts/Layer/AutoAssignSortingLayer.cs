using UnityEngine;

namespace Layer
{
    public class AutoAssignSortingLayer : MonoBehaviour
    {
        [Tooltip("Nhập tên Sorting Layer bạn muốn gán cho tất cả object con. Ví dụ: Elevation_A")]
        public string targetSortingLayer = "Elevation_A";

        private void Start()
        {
            // Luôn đảm bảo layer được áp dụng ngay khi object sinh ra trong game
            ApplyLayer();
        }

        // Nút chức năng sẽ hiện ra khi bạn click chuột phải vào script trong Unity
        [ContextMenu("Apply Layer To All Children")]
        public void ApplyLayer()
        {
            // Quét tìm tất cả SpriteRenderer của object hiện tại và mọi object con bên trong nó
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
            
            foreach (SpriteRenderer sr in renderers)
            {
                sr.sortingLayerName = targetSortingLayer;
            }
            
            // Xử lý thêm cho TilemapRenderer nếu có (vì bạn dùng cả Tilemap)
            var tilemapRenderers = GetComponentsInChildren<UnityEngine.Tilemaps.TilemapRenderer>(true);
            foreach (var tr in tilemapRenderers)
            {
                tr.sortingLayerName = targetSortingLayer;
            }
            
            Debug.Log($"<color=green><b>[SUCCESS]</b></color> Đã tự động cập nhật Sorting Layer <b>'{targetSortingLayer}'</b> cho {renderers.Length} Sprites và {tilemapRenderers.Length} Tilemaps!");
        }

        // Tự động chạy khi bạn thay đổi giá trị trong Inspector
        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                // Bạn có thể bỏ comment dòng dưới nếu muốn nó tự cập nhật mỗi khi gõ tên layer
                // ApplyLayer(); 
            }
        }
    }
}