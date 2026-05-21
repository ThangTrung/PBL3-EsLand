using Core.Contracts.AI;
using Data.Enemies;
using Gameplay.AI.Animation;
using Gameplay.AI.Strategies;
using UnityEngine;

namespace Gameplay.AI.Enemies
{
    public class PandaEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/PandaConfig";
        private const string AnimationPath = "Enemies/Animations/PandaAnims";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            var animConfig = Resources.Load<AnimationConfig>(AnimationPath);

            // Strategy: Combo Melee (Đánh côn nhị khúc)
            var attackStrategy = new ComboMeleeStrategy(
                config?.BaseDamage ?? 8f,
                config?.BaseAttackRange ?? 2.5f,
                config?.AttackCooldown ?? 0.9f,
                3, // 3-hit combo
                Animator,
                transform,
                this);

            InitializeEnemy(config, animConfig, attackStrategy, config?.BaseAttackRange ?? 2.5f);
        }
    }
}
