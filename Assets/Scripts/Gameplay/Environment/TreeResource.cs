using UnityEngine;
using Gameplay.World;

namespace Gameplay.Environment
{
    /// <summary>
    /// Class chuyên biệt cho tài nguyên Cây.
    /// Đã tối ưu: Loại bỏ Update(), sử dụng Override Die() để dọn dẹp.
    /// </summary>
    public class TreeResource : ResourceNode
    {
        [Header("Tree Specific Settings")]
        [SerializeField] private GameObject stumpPrefab;

        protected override void Die()
        {
            // 1. Sinh ra cái gốc cây tại đúng tầng Elevation
            if (stumpPrefab != null)
            {
                Instantiate(stumpPrefab, transform.position, transform.rotation, transform.parent);
            }

            // 2. Gọi logic chết cơ bản (Loot, Disable Colliders, etc.)
            base.Die();

            // 3. Ẩn hoàn toàn xác cây cũ
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
    }
}
