using UnityEngine;
using Infrastructure.Pooling;

namespace Gameplay.World
{
    /// <summary>
    /// Quản lý việc hiển thị các hiệu ứng hình ảnh (Particles) trong game.
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        public static EffectManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject deathEffectPrefab;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void PlayHitEffect(Vector3 position)
        {
            if (hitEffectPrefab == null) return;
            SpawnEffect(hitEffectPrefab, position);
        }

        public void PlayDeathEffect(Vector3 position)
        {
            if (deathEffectPrefab == null) return;
            SpawnEffect(deathEffectPrefab, position);
        }

        private void SpawnEffect(GameObject prefab, Vector3 position)
        {
            // Sử dụng ObjectPoolManager để tối ưu hiệu năng
            var effect = ObjectPoolManager.Instance.Get(prefab, position, Quaternion.identity);
            
            // Nếu effect có ParticleSystem, hãy Play nó (giả sử prefab tự hủy/thu hồi sau khi xong)
            var ps = effect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            
            // Tự động thu hồi về pool sau 2 giây (đảm bảo Particle chạy xong)
            StartCoroutine(ReturnToPoolAfterDelay(effect, 2f));
        }

        private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (obj != null && obj.activeInHierarchy)
            {
                ObjectPoolManager.Instance.Return(obj);
            }
        }
    }
}
