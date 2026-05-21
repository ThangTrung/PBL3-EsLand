using System.Collections.Generic;
using UnityEngine;

namespace Data.Spawning
{
    /// <summary>
    /// Cấu hình cho một đợt quái vật hoặc một hệ sinh thái (Biome).
    /// </summary>
    [CreateAssetMenu(fileName = "WaveConfig", menuName = "EsLand/Spawning/Wave Config")]
    public class WaveConfig : ScriptableObject
    {
        [Header("General Settings")]
        public string waveName = "New Wave";
        
        [Tooltip("Tổng ngân sách (Budget) tối đa cho đợt này. Khi tổng Cost của quái đang active đạt ngưỡng này, Spawner sẽ tạm dừng.")]
        public float totalBudget = 20f;
        
        [Tooltip("Thời gian chờ giữa các lần sinh quái (giây).")]
        public float spawnRate = 2f;

        [Header("Enemy Pool")]
        [Tooltip("Danh sách các loại quái vật có thể xuất hiện trong đợt này.")]
        public List<EnemySpawnData> enemies = new List<EnemySpawnData>();

        /// <summary>
        /// Designer: Hãy điều chỉnh Weight để cân bằng tỉ lệ quái. 
        /// Ví dụ: Goblins (Weight 80) + Minotaur (Weight 5) -> Goblins sẽ xuất hiện áp đảo.
        /// </summary>
        public EnemySpawnData GetRandomEnemy()
        {
            if (enemies == null || enemies.Count == 0) return null;

            int totalWeight = 0;
            foreach (var enemy in enemies) totalWeight += enemy.weight;

            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var enemy in enemies)
            {
                currentWeight += enemy.weight;
                if (randomValue < currentWeight) return enemy;
            }

            return enemies[0];
        }
    }
}
