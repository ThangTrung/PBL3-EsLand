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

        public EnemyBase CreateEnemy(GameObject prefab, IEnemyConfig config, AnimationConfig animConfig, IAttackStrategy attackStrategy, Vector3 position)
        {
            if (prefab == null) return null;

            // Rule 1: Use ObjectPool instead of Instantiate
            GameObject enemyObj = ObjectPoolManager.Instance.Get(prefab, position, Quaternion.identity);
            
            EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.InitializeEnemy(config, animConfig, attackStrategy, config.BaseAttackRange);
                
                if (enemy is IResettable resettable) 
                {
                    resettable.ResetStats(); 
                }

                enemy.OnSpawn();
            }
            
            return enemy;
        }
    }
}
