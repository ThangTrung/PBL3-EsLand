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
                GameObject stump = Instantiate(stumpPrefab, transform.position, transform.rotation, transform.parent);
                
                // Đồng bộ Elevation Layer từ Cây sang Gốc
                var sourceElevation = GetComponent<ElevationAgent>();
                if (sourceElevation != null)
                {
                    var stumpElevation = stump.GetComponent<ElevationAgent>();
                    if (stumpElevation != null)
                    {
                        stumpElevation.ChangeElevation(sourceElevation.CurrentElevation);
                    }
                    else
                    {
                        // Nếu Stump không có ElevationAgent, thử ép thẳng Sorting Layer
                        var layerAssigner = stump.GetComponent<Layer.AutoAssignSortingLayer>();
                        if (layerAssigner != null)
                        {
                            layerAssigner.targetSortingLayer = sourceElevation.CurrentElevation;
                            layerAssigner.ApplyLayer();
                        }
                    }
                }
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