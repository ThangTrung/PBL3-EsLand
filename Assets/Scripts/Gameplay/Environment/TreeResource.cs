using UnityEngine;
using Gameplay.World;

namespace Gameplay.Environment
{
    /// <summary>
    /// Class chuyên biệt cho tài nguyên Cây.
    /// Kế thừa từ ResourceNode để tận dụng logic Damageable và Interactable.
    /// </summary>
    public class TreeResource : ResourceNode
    {
        [Header("Tree Specific Settings")]
        [SerializeField] private GameObject stumpPrefab; // Prefab gốc cây sẽ xuất hiện khi cây đổ

                private void OnEnable()
        {
            // Đăng ký sự kiện chết từ base class
            OnDie += HandleTreeFalling;
        }

        private void OnDisable()
        {
            OnDie -= HandleTreeFalling;
        }

        /// <summary>
        /// Xử lý logic khi cây bị chặt hạ.
        /// </summary>
        private void HandleTreeFalling()
        {
            if (stumpPrefab != null)
            {
                // Sinh ra gốc cây tại vị trí hiện tại của cây
                Instantiate(stumpPrefab, transform.position, transform.rotation, transform.parent);
            }
        }   // Tôi sẽ cần refactor ResourceNode một chút hoặc sử dụng Event OnDie để thực hiện việc sinh Stump.
    }
}
