using UnityEngine;
using Gameplay.World;

namespace Gameplay.Environment
{
    /// <summary>
    /// Class chuyên biệt cho tài nguyên Đá.
    /// Đã tối ưu: Loại bỏ Update(), sử dụng Override Die() để dọn dẹp.
    /// </summary>
    public class RockResource : ResourceNode
    {
        protected override void Die()
        {
            // 1. Gọi logic chết cơ bản
            base.Die();

            // 2. Đảm bảo cục đá tàng hình hoàn toàn để không đè hình
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
    }
}
