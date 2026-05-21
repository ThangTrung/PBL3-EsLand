using Core.Contracts.AI;
using Data.Enemies;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class BearEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/BearConfig";
        private const string AnimationPath = "Enemies/Animations/BearAnims";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);

            // Strategy: Melee (Cào tát)
            var attackStrategy = new MeleeAttackStrategy(
                config?.BaseDamage ?? 9f,
                config?.BaseAttackRange ?? 2.8f,
                config?.AttackCooldown ?? 1.5f,
                Animator,
                animConfig?.AttackTriggerFrame ?? 2,
                transform,
                this);

            InitializeEnemy(config, animConfig, attackStrategy, config?.BaseAttackRange ?? 2.8f);
        }
    }
}
