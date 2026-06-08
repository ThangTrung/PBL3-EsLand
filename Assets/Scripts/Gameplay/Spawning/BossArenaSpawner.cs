using UnityEngine;
using Core.Contracts.Spawning;
using Gameplay.AI;
using Gameplay.AI.Factories;
using Data.Spawning;
using Infrastructure.SaveSystem.Core;
using Infrastructure.SaveSystem.Data;

namespace Gameplay.Spawning
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(SaveableEntity))]
    public class BossArenaSpawner : MonoBehaviour, ISaveable
    {
        [Header("Arena Configuration")]
        [Tooltip("Dữ liệu Boss cần sinh (tương tự WaveConfig)")]
        [SerializeField] private EnemySpawnData bossData;
        
        [Tooltip("Vị trí Boss sẽ xuất hiện (Nếu trống sẽ lấy vị trí của Trigger)")]
        [SerializeField] private Transform spawnPoint;

        private SaveableEntity _saveableEntity;
        private bool _isDefeated = false;
        private bool _isSpawned = false;
        
        private EnemyBase _activeBoss;
        private ISpawnCondition[] _conditions;

        private void Awake()
        {
            _saveableEntity = GetComponent<SaveableEntity>();
            
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
            
            // Lấy tất cả các điều kiện được gắn trên GameObject này hoặc object con
            _conditions = GetComponentsInChildren<ISpawnCondition>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isDefeated || _isSpawned) return;

            if (other.CompareTag("Player"))
            {
                if (CanSpawnBoss(other.transform))
                {
                    SpawnBoss(other.transform);
                }
                else
                {
                    // Lấy feedback từ điều kiện đầu tiên không thỏa mãn
                    foreach (var cond in _conditions)
                    {
                        if (!cond.IsMet(other.transform))
                        {
                            Debug.Log($"[Boss Arena] {cond.GetFeedbackMessage()}");
                            // TODO: Bắn Event UI để hiển thị lên màn hình người chơi
                            break;
                        }
                    }
                }
            }
        }

        private bool CanSpawnBoss(Transform player)
        {
            if (_conditions == null || _conditions.Length == 0) return true;
            
            foreach (var cond in _conditions)
            {
                if (!cond.IsMet(player)) return false;
            }
            return true;
        }

        private void SpawnBoss(Transform playerTransform)
        {
            if (bossData == null || bossData.prefab == null)
            {
                Debug.LogError("[Boss Arena] Boss Data is missing!");
                return;
            }

            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;

            _activeBoss = EnemyFactory.Instance.CreateEnemy(
                bossData.prefab,
                bossData.config,
                bossData.animConfig,
                null,
                pos,
                transform.parent // [FIX] Truyền parent để hỗ trợ kế thừa Elevation Layer
            );

            if (_activeBoss != null)
            {
                _activeBoss.SetTarget(playerTransform);
                _isSpawned = true;
                
                // Đăng ký lắng nghe sự kiện chết của Boss
                var health = _activeBoss.GetComponent<Gameplay.Characters.CharacterHealth>();
                if (health != null)
                {
                    health.OnDie += HandleBossDefeated;
                }
                else
                {
                    Debug.LogWarning("[Boss Arena] Boss does not have CharacterHealth component.");
                }
            }
        }

        private void HandleBossDefeated()
        {
            _isDefeated = true;
            
            // Hủy đăng ký để tránh memory leak
            if (_activeBoss != null)
            {
                var health = _activeBoss.GetComponent<Gameplay.Characters.CharacterHealth>();
                if (health != null)
                {
                    health.OnDie -= HandleBossDefeated;
                }
            }
            
            Debug.Log("[Boss Arena] Boss Defeated! Arena is now cleared.");
        }

        #region ISaveable Implementation

        public void SaveData(GameData data)
        {
            // Nếu Boss đã bị tiêu diệt, lưu ID của Arena vào danh sách đã bị phá hủy
            if (_isDefeated && !string.IsNullOrEmpty(_saveableEntity.Id))
            {
                if (!data.destroyedEntityIDs.Contains(_saveableEntity.Id))
                {
                    data.destroyedEntityIDs.Add(_saveableEntity.Id);
                }
            }
        }

        public void LoadData(GameData data)
        {
            if (string.IsNullOrEmpty(_saveableEntity.Id)) return;

            // Nếu ID của Arena nằm trong danh sách đã phá hủy -> Vùng này đã qua
            if (data.destroyedEntityIDs.Contains(_saveableEntity.Id))
            {
                _isDefeated = true;
                _isSpawned = true; // Không cho spawn nữa
                gameObject.SetActive(false); // Tắt luôn trigger
            }
        }

        #endregion
    }
}