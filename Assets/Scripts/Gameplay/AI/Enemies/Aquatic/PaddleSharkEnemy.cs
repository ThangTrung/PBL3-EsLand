using Core.Contracts.AI;
using Data.Enemies;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using UnityEngine;

namespace Gameplay.AI.Enemies.Aquatic
{
    public class PaddleSharkEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/PaddleSharkConfig";
        private const string AnimationPath = "Enemies/Animations/PaddleSharkAnims";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);

            // Strategy: Melee with Knockback (Heavy attack)
            var attackStrategy = new HeavyBluntStrategy(
                config?.BaseDamage ?? 5f,
                config?.BaseAttackRange ?? 2.2f,
                config?.AttackCooldown ?? 1.2f,
                4f, // Knockback force
                Animator,
                transform,
                this);

            InitializeEnemy(config, animConfig, attackStrategy, config?.BaseAttackRange ?? 2.2f);
        }
    }
}
