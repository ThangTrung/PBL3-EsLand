using UnityEngine;
using Gameplay.Spawning;
using Core.Contracts.Environment;

namespace Gameplay.Environment
{
    public class ArenaLockdownManager : MonoBehaviour
    {
        [Tooltip("Kéo BossArenaSpawner vào đây để lắng nghe sự kiện")]
        [SerializeField] private BossArenaSpawner arenaSpawner;
        
        private IArenaBarrier[] _barriers;

        private void Awake()
        {
            if (arenaSpawner == null)
            {
                arenaSpawner = GetComponentInParent<BossArenaSpawner>();
            }
            
            // Tự động tìm tất cả các rào chắn có interface IArenaBarrier
            _barriers = GetComponentsInChildren<IArenaBarrier>(true);
            
            // Mở khóa rào chắn lúc mới vào map
            SetBarriersLocked(false);
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
            SetBarriersLocked(true);
        }

        private void HandleBossDefeated()
        {
            SetBarriersLocked(false);
        }

        private void SetBarriersLocked(bool isLocked)
        {
            if (_barriers == null) return;
            foreach (var barrier in _barriers)
            {
                if (barrier != null)
                {
                    if (isLocked)
                        barrier.Lock();
                    else
                        barrier.Unlock();
                }
            }
        }
    }
}