using Core.Contracts.AI;
using Core.Contracts.Combat;
using Gameplay.AI.Animation;
using Gameplay.AI.Factories;
using Gameplay.AI.Strategies;
using Infrastructure.Pooling;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class WildBoarEnemy : EnemyBase, IHexable
    {
        [Header("Hex Transformation")]
        [SerializeField] private GameObject mountedSpearGoblinPrefab;
        [SerializeField] private ScriptableObject mountedGoblinConfig; // Cast to IEnemyConfig in code
        [SerializeField] private AnimationConfig mountedGoblinAnimConfig;

        public override void InitializeEnemy(IEnemyConfig config, AnimationConfig animationConfig, IAttackStrategy attackStrategy, float attackRange)
        {
            var strategy = attackStrategy ?? new ChargeAttackStrategy(
                config.BaseDamage,
                attackRange,
                config.AttackCooldown,
                Animator,
                transform,
                this);

            base.InitializeEnemy(config, animationConfig, strategy, attackRange);
        }

        public void OnHexed()
        {
            Debug.Log("Boar is being hexed into a Mounted Spear Goblin!");
            
            Vector3 spawnPos = transform.position;
            
            // 1. Prepare for Swap (Disable Loot/Death events)
            PrepareForEntitySwap();

            // 2. Return Boar to Pool
            ObjectPoolManager.Instance.Return(gameObject);

            // 2. Spawn Mounted Spear Goblin
            if (mountedSpearGoblinPrefab != null && mountedGoblinConfig is IEnemyConfig cfg)
            {
                EnemyFactory.Instance.CreateEnemy(mountedSpearGoblinPrefab, cfg, mountedGoblinAnimConfig, null, spawnPos);
            }
        }
    }
}