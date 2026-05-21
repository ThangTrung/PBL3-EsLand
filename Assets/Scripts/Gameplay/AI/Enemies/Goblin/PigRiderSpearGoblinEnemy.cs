using Core.Contracts.AI;
using Data.Enemies;
using Gameplay.AI.Animation;
using Gameplay.AI.Movement;
using Gameplay.AI.Strategies;
using UnityEngine;

namespace Gameplay.AI.Enemies.Goblin
{
    public class PigRiderSpearGoblinEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/PigRiderConfig";
        private const string AnimationPath = "Enemies/Animations/PigRiderAnims";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);

            // Strategy: Drive By Slash (Charge through and slash)
            var attackStrategy = new DriveBySlashStrategy(
                config?.BaseDamage ?? 8f,
                config?.BaseAttackRange ?? 2.5f,
                config?.AttackCooldown ?? 1.3f,
                1.5f, // Speed boost multiplier
                Animator,
                GetComponent<EnemyMovementController>(),
                transform,
                this);

            InitializeEnemy(config, animConfig, attackStrategy, config?.BaseAttackRange ?? 2.5f);
        }
    }
}
