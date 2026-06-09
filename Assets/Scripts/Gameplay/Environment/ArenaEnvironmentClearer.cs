using UnityEngine;
using Core.Contracts.Shared;
using Infrastructure.Pooling;
using Gameplay.World; // Giả sử Resource node nằm trong đây

namespace Gameplay.Environment
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ArenaEnvironmentClearer : MonoBehaviour
    {
        [Tooltip("Các Layer chứa cây cối, đá, quặng cần bị dọn dẹp khỏi khu vực Arena")]
        [SerializeField] private LayerMask layersToClear;

        [Tooltip("Có tự động dọn dẹp khi mới bật không? (Mặc định: Bật)")]
        [SerializeField] private bool clearOnStart = true;

        private BoxCollider2D _arenaCollider;

        private void Awake()
        {
            _arenaCollider = GetComponent<BoxCollider2D>();
        }

        private void Start()
        {
            if (clearOnStart)
            {
                ClearEnvironment();
            }
        }

        public void ClearEnvironment()
        {
            if (_arenaCollider == null) return;

            // Quét các vật thể nằm trong phạm vi của BoxCollider2D
            Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
                _arenaCollider.bounds.center,
                _arenaCollider.bounds.size,
                0f,
                layersToClear
            );

            foreach (var col in hitColliders)
            {
                // Bỏ qua nếu là Player hoặc Enemy hoặc chính Arena Trigger
                if (col.CompareTag("Player") || col.CompareTag("Enemy") || col.gameObject == this.gameObject)
                    continue;

                // [GLOBAL SAFETY FIX] KHÔNG bao giờ được xóa Tilemap.
                // Một Tilemap là object bao trùm toàn bộ bản đồ. Việc xóa nó sẽ làm hỏng toàn bộ map.
                if (col.GetComponent<UnityEngine.Tilemaps.Tilemap>() != null || col.GetComponent<CompositeCollider2D>() != null)
                {
                    Debug.Log($"[ArenaClearer] Đã bỏ qua vật thể nền tảng: {col.gameObject.name} (Tilemap/Composite)");
                    continue;
                }

                // Nếu là đối tượng từ Object Pool, hãy trả nó về Pool để tối ưu hiệu suất
                var poolable = col.GetComponent<IPoolable>();
                if (poolable != null)
                {
                    ObjectPoolManager.Instance.ReturnToPool(col.gameObject);
                }
                else
                {
                    // Nếu không phải Pool, phá hủy bình thường
                    Destroy(col.gameObject);
                }
            }
        }
    }
}