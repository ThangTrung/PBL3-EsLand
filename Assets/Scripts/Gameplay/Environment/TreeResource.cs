using UnityEngine;
using Gameplay.World;

namespace Gameplay.Environment
{
    /// <summary>
    /// Class chuyên biệt cho tài nguyên Cây.
    /// Đã fix triệt để lỗi đè gốc cây và tường tàng hình (BoxCollider).
    /// </summary>
    public class TreeResource : ResourceNode
    {
        [Header("Tree Specific Settings")]
        [SerializeField] private GameObject stumpPrefab;
        
        // Biến đánh dấu để chỉ thực hiện tàng hình 1 lần duy nhất
        private bool _isGhosted = false;

        private void Update()
        {
            // Kiểm tra: Nếu cây đã chết (do bị chặt HOẶC do Load Game) và chưa bị tàng hình
            if (IsDead && !_isGhosted)
            {
                GhostTreeAndSpawnPrefab();
                _isGhosted = true;
            }
        }

        private void GhostTreeAndSpawnPrefab()
        {
            // 1. Sinh ra cái gốc xịn của ông
            if (stumpPrefab != null)
            {
                // [GLOBAL FIX] Instantiate với parent là transform.parent (thường là Elevation_A/B/C)
                // Việc này giúp script AutoAssignSortingLayer trên gốc cây tự động kế thừa đúng layer.
                Instantiate(stumpPrefab, transform.position, transform.rotation, transform.parent);
            }

            // 2. Tắt hiển thị của cái xác cây cũ (tránh lớp cha tráo hình đè lên)
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) 
            {
                sr.enabled = false;
            }

            // 3. XỬ BẮN TẤT CẢ COLLIDER: Quét sạch cả Polygon lẫn BoxCollider cản đường
            foreach (Collider2D col in GetComponents<Collider2D>())
            {
                col.enabled = false;
            }
        }
    }
}