using UnityEngine;
using Gameplay.World;

namespace Gameplay.Environment
{
    /// <summary>
    /// Class chuyên biệt cho tài nguyên Đá.
    /// Đã fix triệt để lỗi tường tàng hình sau khi đập nát.
    /// </summary>
    public class RockResource : ResourceNode
    {
        // Biến đánh dấu để chỉ thực hiện tắt 1 lần duy nhất
        private bool _isGhosted = false;

        private void Update()
        {
            // Nếu đá đã vỡ (chết) và chưa được dọn dẹp
            if (IsDead && !_isGhosted)
            {
                CleanUpRock();
                _isGhosted = true;
            }
        }

        private void CleanUpRock()
        {
            // 1. Quét sạch TẤT CẢ Collider đang có trên cục đá và gạt công tắc Tắt
            foreach (Collider2D col in GetComponents<Collider2D>())
            {
                col.enabled = false;
            }

            // 2. (Tùy chọn) Đảm bảo cục đá tàng hình hoàn toàn để không đè hình
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) 
            {
                sr.enabled = false;
            }
            
            // Có thể thêm hiệu ứng particle bụi đá rớt ra ở đây sau này
        }
    }
}