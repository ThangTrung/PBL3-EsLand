using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data.Spawning;
using Gameplay.AI;
using Gameplay.AI.Factories;
using Infrastructure.Pooling;
using Core.Events;

namespace Gameplay.Spawning
{
    /// <summary>
    /// AI Director điều phối việc sinh và hủy quái vật tối ưu cho Map rộng (Open World).
    /// Sử dụng cơ chế Auto-Registration và Spatial Partitioning.
    /// </summary>
    public class EnemySpawnDirector : MonoBehaviour
    {
        #region Constants & Config
        private const float DeadZoneRadiusSqr = 12f * 12f;    // 12m (Gần quá không sinh)
        private const float DespawnRadiusSqr = 45f * 45f;     // 45m (Xa quá thu hồi)
        private const float LazyTickInterval = 1.0f;
        #endregion

        #region Singleton
        public static EnemySpawnDirector Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        #endregion

        #region Serialized Fields
        [Header("Global Settings")]
        [SerializeField] private WaveConfig defaultWave;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private LayerMask spawnAreaLayer;

        [Header("Performance Monitor")]
        [SerializeField] private float currentBudget;
        [SerializeField] private int activeCount;
        
        private List<EnemyBase> activeEnemies = new List<EnemyBase>();
        private Dictionary<EnemyBase, float> _enemyCosts = new Dictionary<EnemyBase, float>();
        
        // Tất cả Area hiện có (để fallback hoặc debug)
        private HashSet<SpawnArea> _registeredAreas = new HashSet<SpawnArea>();
        #endregion

        private List<SpawnArea> _activeAreas = new List<SpawnArea>();
        private float _spawnTimer;

        public void SetAreaActive(SpawnArea area, bool isActive)
        {
            if (isActive)
            {
                if (!_activeAreas.Contains(area)) _activeAreas.Add(area);
            }
            else
            {
                _activeAreas.Remove(area);
            }
        }

        private void Update()
        {
            // Update playerTransform reference dynamically
            if (playerTransform == null)
            {
                playerTransform = Gameplay.Characters.TargetTracker.PlayerTarget;
            }

            if (playerTransform == null) return;

            _spawnTimer += Time.deltaTime;
            
            // Lấy WaveConfig hiện tại (Ưu tiên Area cuối cùng được kích hoạt, nếu không dùng Default)
            SpawnArea currentArea = _activeAreas.Count > 0 ? _activeAreas[_activeAreas.Count - 1] : null;
            WaveConfig targetWave = (currentArea != null && currentArea.LocalWaveConfig != null) 
                                    ? currentArea.LocalWaveConfig 
                                    : defaultWave;

            if (targetWave == null) return;

            if (_spawnTimer >= targetWave.spawnRate)
            {
                _spawnTimer = 0f;
                TrySpawnEnemy(targetWave, currentArea);
            }
        }

        #region Logic Spawning
        private void TrySpawnEnemy(WaveConfig wave, SpawnArea area)
        {
            if (currentBudget >= wave.totalBudget) return;

            EnemySpawnData spawnData = wave.GetRandomEnemy();
            if (spawnData == null) return;

            // Nếu không có Area được báo cáo, tìm Area gần nhất (fallback)
            if (area == null)
            {
                area = GetFallbackArea();
            }
            
            if (area == null) return;

            Vector3 spawnPos = area.GetValidSpawnPoint();
            if (spawnPos == Vector3.zero) return;

            EnemyBase enemy = EnemyFactory.Instance.CreateEnemy(
                spawnData.prefab, 
                spawnData.config, 
                spawnData.animConfig, 
                null, 
                spawnPos,
                area.transform
            );

            if (enemy != null)
            {
                RegisterEnemy(enemy, spawnData.cost);
            }
        }

        private SpawnArea GetFallbackArea()
        {
            // Fallback cực nhanh nếu không có trigger nào trúng
            foreach (var area in _registeredAreas)
            {
                if (area == null) continue;
                float distSqr = (area.transform.position - playerTransform.position).sqrMagnitude;
                if (distSqr < DespawnRadiusSqr) return area;
            }
            return null;
        }

        private void RegisterEnemy(EnemyBase enemy, float cost)
        {
            if (enemy == null) return;
            
            if (!activeEnemies.Contains(enemy))
            {
                activeEnemies.Add(enemy);
                _enemyCosts[enemy] = cost;
                currentBudget += cost;
                activeCount = activeEnemies.Count;
            }
        }

        private void HandleEnemyDied(EnemyBase enemy)
        {
            if (enemy == null) return;
            UnregisterEnemy(enemy);
        }

        public void UnregisterEnemy(EnemyBase enemy)
        {
            if (enemy == null) return;

            if (_enemyCosts.TryGetValue(enemy, out float cost))
            {
                currentBudget = Mathf.Max(0, currentBudget - cost);
                _enemyCosts.Remove(enemy);
            }

            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
                activeCount = activeEnemies.Count;
            }
        }
        #endregion

        #region Performance Optimization (Lazy Tick)
        private IEnumerator LazyTickRoutine()
        {
            var wait = new WaitForSeconds(LazyTickInterval);
            while (isActiveAndEnabled)
            {
                yield return wait;
                if (playerTransform == null) continue;

                Vector3 playerPos = playerTransform.position;
                
                for (int i = activeEnemies.Count - 1; i >= 0; i--)
                {
                    var enemy = activeEnemies[i];
                    if (enemy == null) { activeEnemies.RemoveAt(i); continue; }

                    if (!enemy.gameObject.activeInHierarchy)
                    {
                        UnregisterEnemy(enemy);
                        continue;
                    }

                    // Nếu quái ra khỏi Vòng Despawn (45m), thu hồi về Pool
                    float distSqr = (enemy.transform.position - playerPos).sqrMagnitude;
                    if (distSqr > DespawnRadiusSqr)
                    {
                        DespawnEnemy(enemy);
                    }
                }
            }
        }

        private void DespawnEnemy(EnemyBase enemy)
        {
            if (enemy == null) return;
            UnregisterEnemy(enemy);
            ObjectPoolManager.Instance.Return(enemy.gameObject);
        }
        #endregion

        #region Area Registration
        public void ManualRegisterSpawnArea(SpawnArea area)
        {
            if (area != null) _registeredAreas.Add(area);
        }

        public void UnregisterSpawnArea(SpawnArea area)
        {
            if (area != null && _registeredAreas.Contains(area)) _registeredAreas.Remove(area);
        }
        #endregion
    }
}