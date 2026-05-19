using Core.Contracts.AI;
using Data.Enemies;
using Gameplay.AI.Animation;using Gameplay.AI.Movement;

using Gameplay.AI.Strategies;
using Gameplay.Characters;
using UnityEngine;

namespace Gameplay.AI.Enemies.Goblin
{
    public class LancerEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/GoblinLancerConfig";
        private const string AnimationPath = "Enemies/Animations/GoblinLancerAnims";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            if (config == null)
            {
                Debug.LogWarning($"[LancerEnemy] Missing config at Resources/{ConfigPath}. Using fallback defaults.");
                config = ScriptableObject.CreateInstance<SimpleEnemyConfig>();
                config.Initialize("GoblinLancer", 28f, 6f, 3f, 12f, 5f, 1.1f, 2.8f, Color.white);
            }

            var animationConfig = Resources.Load<AnimationConfig>(AnimationPath);
            if (animationConfig == null)
            {
                Debug.LogWarning($"[LancerEnemy] Missing animation config at Resources/{AnimationPath}. Create one to enable animations.");
                animationConfig = ScriptableObject.CreateInstance<AnimationConfig>();
                animationConfig.Initialize(null, null, null, null, 10f, 2);
            }

            var health = GetComponent<CharacterHealth>();
            if (health != null)
            {
                health.SetMaxHealth(config.MaxHealth, true);
            }

            var movementController = GetComponent<EnemyMovementController>();
            if (movementController != null)
            {
                movementController.SetBaseMoveSpeed(config.MoveSpeed);
            }

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = config.TintColor;
            }

            var attackStrategy = new MeleeAttackStrategy(
                config.BaseDamage,
                config.BaseAttackRange,
                config.AttackCooldown,
                Animator,
                animationConfig.AttackTriggerFrame,
                transform,
                this);

            InitializeEnemy(config, animationConfig, attackStrategy, config.BaseAttackRange);
        }
    }
}
