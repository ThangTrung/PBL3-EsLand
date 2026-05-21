using UnityEngine;

namespace Gameplay.Spawning
{
    /// <summary>
    /// Đánh dấu một vùng không gian hợp lệ để sinh quái vật.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SpawnArea : MonoBehaviour
    {
        [Header("Area Settings")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float checkRadius = 0.5f;

        private Collider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            // Đảm bảo collider là Trigger để không gây va chạm vật lý
            _collider.isTrigger = true;
        }

        /// <summary>
        /// Tìm một vị trí trống ngẫu nhiên bên trong vùng.
        /// </summary>
        /// <param name="maxAttempts">Số lần thử tìm điểm trống tối đa.</param>
        /// <returns>Vị trí hợp lệ hoặc Vector3.zero nếu thất bại.</returns>
        public Vector3 GetValidSpawnPoint(int maxAttempts = 10)
        {
            if (_collider == null) return transform.position;

            Bounds bounds = _collider.bounds;

            for (int i = 0; i < maxAttempts; i++)
            {
                // Lấy điểm ngẫu nhiên trong Bounds
                Vector3 randomPoint = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    0f
                );

                // Kiểm tra xem điểm đó có nằm TRONG Collider không (dành cho PolygonCollider2D)
                if (!_collider.OverlapPoint(randomPoint)) continue;

                // Kiểm tra xem có bị đè lên vật cản (Tường, Đá, Cây) không
                Collider2D hit = Physics2D.OverlapCircle(randomPoint, checkRadius, obstacleLayer);
                
                if (hit == null)
                {
                    return randomPoint;
                }
            }

            return Vector3.zero; // Không tìm được điểm trống
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_collider == null) _collider = GetComponent<Collider2D>();
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(_collider.bounds.center, _collider.bounds.size);
        }
        #endif
    }
}
