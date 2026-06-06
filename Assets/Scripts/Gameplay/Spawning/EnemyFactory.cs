using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Infrastructure.Pooling;
using UnityEngine;

namespace Gameplay.AI.Factories
{
    public class EnemyFactory : MonoBehaviour
    {
        private static EnemyFactory _instance;
        public static EnemyFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<EnemyFactory>();
                    if (_instance == null)
                    {
                        var go = new GameObject("EnemyFactory");
                        _instance = go.AddComponent<EnemyFactory>();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Sinh quái từ EnemySpawnData (Dùng cho Smart Spawner).
        /// </summary>
        public EnemyBase CreateEnemy(Data.Spawning.EnemySpawnData data, Vector3 position, Transform parent = null)
        {
            if (data == null || data.prefab == null) return null;

            // 1. Lấy từ Pool (Sử dụng prefab của quái vật)
            GameObject enemyObj = ObjectPoolManager.Instance.Get(data.prefab.gameObject, position, parent);
            
            EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                // 2. [FIX] Đảm bảo gọi InitializeEnemy với đầy đủ dữ liệu từ data
                // Nếu attackStrategy truyền vào là null, EnemyBase sẽ tự quyết định theo logic class của nó
                enemy.InitializeEnemy(data.config, data.animConfig, null, data.config != null ? data.config.BaseAttackRange : 2f);
                
                // 3. Reset state/máu
                enemy.ResetEnemy();
                
                // 4. Thông báo sinh thành công
                enemy.OnSpawn();
            }
            
            return enemy;
        }

        public EnemyBase CreateEnemy(GameObject prefab, IEnemyConfig config, AnimationConfig animConfig, IAttackStrategy attackStrategy, Vector3 position, Transform parent = null)
        {
            if (prefab == null) return null;

            GameObject enemyObj = ObjectPoolManager.Instance.Get(prefab, position, Quaternion.identity, parent);
            
            EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.InitializeEnemy(config, animConfig, attackStrategy, config != null ? config.BaseAttackRange : 2f);
                enemy.ResetEnemy();
                enemy.OnSpawn();
            }
            
            return enemy;
        }
    }
}