using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data.Spawning;
using Gameplay.AI;
using Gameplay.AI.Factories;
using Infrastructure.Pooling;

namespace Gameplay.Spawning
{
    /// <summary>
    /// AI Director điều phối việc sinh và hủy quái vật dựa trên vị trí Player và hiệu suất.
    /// </summary>
    public class EnemySpawnDirector : MonoBehaviour
    {
        #region Constants & Config
        private const float DeadZoneRadiusSqr = 10f * 10f;    // 10m
        private const float ActiveZoneRadiusSqr = 25f * 25f;  // 25m
        private const float DespawnRadiusSqr = 35f * 35f;     // 35m
        private const float LazyTickInterval = 1.5f;          // Tần suất check quái ở xa
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
        [Header("Director Settings")]
        [SerializeField] private WaveConfig currentWave;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private List<SpawnArea> spawnAreas = new List<SpawnArea>();

        [Header("Performance Monitor")]
        [SerializeField] private float currentBudget;
        [SerializeField] private List<EnemyBase> activeEnemies = new List<EnemyBase>();
        #endregion

        private float _spawnTimer;

        private void Start()
        {
            if (playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }

            // Bắt đầu chu kỳ Lazy Tick để tối ưu CPU
            StartCoroutine(LazyTickRoutine());
        }

        private void Update()
        {
            if (currentWave == null || playerTransform == null) return;

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= currentWave.spawnRate)
            {
                _spawnTimer = 0f;
                TrySpawnEnemy();
            }
        }

        #region Logic Spawning
        private void TrySpawnEnemy()
        {
            // Kiểm tra hạn mức ngân sách (Quota)
            if (currentBudget >= currentWave.totalBudget) return;

            // 1. Chọn quái ngẫu nhiên từ cấu hình đợt
            EnemySpawnData spawnData = currentWave.GetRandomEnemy();
            if (spawnData == null) return;

            // 2. Tìm khu vực sinh sản (SpawnArea) nằm trong Vòng Active (10-25m)
            SpawnArea targetArea = FindActiveSpawnArea();
            if (targetArea == null) return;

            // 3. Lấy điểm trống trong Area
            Vector3 spawnPos = targetArea.GetValidSpawnPoint();
            if (spawnPos == Vector3.zero) return;

            // 4. Sinh quái thông qua Factory (Đảm bảo Object Pooling)
            // LƯU Ý: Phải truyền đầy đủ config từ spawnData
            EnemyBase enemy = EnemyFactory.Instance.CreateEnemy(
                spawnData.prefab, 
                spawnData.config, 
                spawnData.animConfig, 
                null, // Strategy sẽ được script quái tự khởi tạo nếu truyền null
                spawnPos
            );

            if (enemy != null)
            {
                RegisterEnemy(enemy, spawnData.cost);
            }
        }

        private SpawnArea FindActiveSpawnArea()
        {
            if (spawnAreas.Count == 0) return null;

            List<SpawnArea> validAreas = new List<SpawnArea>();
            Vector3 playerPos = playerTransform.position;

            foreach (var area in spawnAreas)
            {
                float distSqr = (area.transform.position - playerPos).sqrMagnitude;

                // CHỈ sinh trong vùng Active (10-25m)
                if (distSqr >= DeadZoneRadiusSqr && distSqr <= ActiveZoneRadiusSqr)
                {
                    validAreas.Add(area);
                }
            }

            if (validAreas.Count == 0) return null;
            return validAreas[Random.Range(0, validAreas.Count)];
        }

        private void RegisterEnemy(EnemyBase enemy, float cost)
        {
            activeEnemies.Add(enemy);
            currentBudget += cost;

            // Lắng nghe sự kiện chết để hoàn trả Budget
            // Cần một cách để lưu trữ Cost của từng con quái (VD: dùng Dictionary)
        }
        #endregion

        #region Performance Optimization (Lazy Tick)
        private IEnumerator LazyTickRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(LazyTickInterval);
                if (playerTransform == null) continue;

                Vector3 playerPos = playerTransform.position;
                
                // Duyệt ngược để an toàn khi xóa phần tử
                for (int i = activeEnemies.Count - 1; i >= 0; i--)
                {
                    var enemy = activeEnemies[i];
                    if (enemy == null || !enemy.gameObject.activeInHierarchy)
                    {
                        activeEnemies.RemoveAt(i);
                        continue;
                    }

                    // Nếu quái ra khỏi Vòng 3 (35m), thu hồi về Pool
                    float distSqr = (enemy.transform.position - playerPos).sqrMagnitude;
                    if (distSqr > DespawnRadiusSqr)
                    {
                        DespawnEnemy(enemy, i);
                    }
                }
            }
        }

        private void DespawnEnemy(EnemyBase enemy, int index)
        {
            // Cần trừ Budget ở đây (Giả định cost cố định cho demo hoặc lưu trữ meta-data)
            currentBudget = Mathf.Max(0, currentBudget - 1f); // Cần refactor để lấy cost chuẩn
            activeEnemies.RemoveAt(index);
            
            // Trả về Pool ngay lập tức
            ObjectPoolManager.Instance.Return(enemy.gameObject);
        }
        #endregion

        #region Helper
                public void UnregisterEnemy(EnemyBase enemy)
        {
            if (activeEnemies.Contains(enemy)) activeEnemies.Remove(enemy);
        }
public void ManualRegisterSpawnArea(SpawnArea area)
        {
            if (!spawnAreas.Contains(area)) spawnAreas.Add(area);
        }
        #endregion
    }
}
