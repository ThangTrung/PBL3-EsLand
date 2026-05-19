using Core.Contracts.AI;
using Gameplay.AI.Animation;
using Gameplay.AI.Factories;
using Gameplay.AI.Strategies;
using Infrastructure.Pooling;
using UnityEngine;
using Gameplay.Characters;

namespace Gameplay.AI.Enemies
{
    public class MountedSpearGoblinEnemy : EnemyBase
    {
        [Header("Death Spawn")]
        [SerializeField] private GameObject lancerPrefab;
        [SerializeField] private ScriptableObject lancerConfig; 
        [SerializeField] private AnimationConfig lancerAnimConfig;

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

        public override void ResetStats()
        {
            base.ResetStats();
        }

        // When this unit dies, it spawns a regular Lancer
        protected override void Awake()
        {
            base.Awake();
            if (TryGetComponent<CharacterHealth>(out var health))
            {
                health.OnDie += HandleLancerSpawn;
            }
        }

        protected override void OnDestroy()
        {
            if (TryGetComponent<CharacterHealth>(out var health))
            {
                health.OnDie -= HandleLancerSpawn;
            }
            base.OnDestroy();
        }

        // We should hook into the Die logic to spawn the Lancer
        private void HandleLancerSpawn()
        {
            if (lancerPrefab != null && lancerConfig is IEnemyConfig cfg)
            {
                EnemyFactory.Instance.CreateEnemy(lancerPrefab, cfg, lancerAnimConfig, null, transform.position);
            }
        }
    }
}