using UnityEngine;
using Data.Spawning;

namespace Gameplay.Spawning
{
    /// <summary>
    /// Đánh dấu một vùng không gian hợp lệ để sinh quái vật.
    /// Hỗ trợ Map rộng bằng cách tự động đăng ký với Director.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SpawnArea : MonoBehaviour
    {
        [Header("Area Settings")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float checkRadius = 0.5f;
        
        [Header("Local Ecosystem (Optional)")]
        [Tooltip("Nếu để trống, sẽ dùng WaveConfig mặc định của Director.")]
        [SerializeField] private WaveConfig localWaveConfig;

        private Collider2D _collider;

        public WaveConfig LocalWaveConfig => localWaveConfig;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
        }

        private void OnEnable()
        {
            // Tự động đăng ký khi được kích hoạt trong Scene
            if (EnemySpawnDirector.Instance != null)
            {
                EnemySpawnDirector.Instance.ManualRegisterSpawnArea(this);
            }
        }

        private void OnDisable()
        {
            // Tự động hủy đăng ký khi bị tắt/xóa
            if (EnemySpawnDirector.Instance != null)
            {
                EnemySpawnDirector.Instance.UnregisterSpawnArea(this);
            }
        }

        public Vector3 GetValidSpawnPoint(int maxAttempts = 10)
        {
            if (_collider == null) return transform.position;

            Bounds bounds = _collider.bounds;

            for (int i = 0; i < maxAttempts; i++)
            {
                Vector3 randomPoint = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    0f
                );

                if (!_collider.OverlapPoint(randomPoint)) continue;

                Collider2D hit = Physics2D.OverlapCircle(randomPoint, checkRadius, obstacleLayer);
                if (hit == null) return randomPoint;
            }

            return Vector3.zero;
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_collider == null) _collider = GetComponent<Collider2D>();
            Gizmos.color = localWaveConfig != null ? new Color(1, 0.5f, 0, 0.4f) : new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(_collider.bounds.center, _collider.bounds.size);
        }
        #endif
    }
}