using UnityEngine;
using Gameplay.Spawning;

namespace Gameplay.Environment
{
    public class ArenaLockdownManager : MonoBehaviour
    {
        [Tooltip("Kéo BossArenaSpawner vào đây để lắng nghe sự kiện")]
        [SerializeField] private BossArenaSpawner arenaSpawner;
        
        [Tooltip("Danh sách các rào chắn (tường vô hình/cửa) cần kích hoạt khi đánh Boss")]
        [SerializeField] private GameObject[] barriers;

        private void Awake()
        {
            if (arenaSpawner == null)
            {
                arenaSpawner = GetComponentInParent<BossArenaSpawner>();
            }
            
            // Tắt rào chắn lúc mới vào map
            SetBarriersActive(false);
        }

        private void OnEnable()
        {
            if (arenaSpawner != null)
            {
                arenaSpawner.OnBossSpawnedEvent += HandleBossSpawned;
                arenaSpawner.OnBossDefeatedEvent += HandleBossDefeated;
            }
        }

        private void OnDisable()
        {
            if (arenaSpawner != null)
            {
                arenaSpawner.OnBossSpawnedEvent -= HandleBossSpawned;
                arenaSpawner.OnBossDefeatedEvent -= HandleBossDefeated;
            }
        }

        private void HandleBossSpawned()
        {
            SetBarriersActive(true);
        }

        private void HandleBossDefeated()
        {
            SetBarriersActive(false);
        }

        private void SetBarriersActive(bool isActive)
        {
            if (barriers == null) return;
            foreach (var barrier in barriers)
            {
                if (barrier != null)
                {
                    barrier.SetActive(isActive);
                }
            }
        }
    }
}