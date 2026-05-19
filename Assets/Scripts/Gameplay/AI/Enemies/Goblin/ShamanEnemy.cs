using Core.Contracts.AI;
using Data.Combat;
using Data.Enemies;
using Gameplay.AI.Animation;
using Gameplay.AI.States;
using Gameplay.AI.Strategies;
using Gameplay.Characters;
using Gameplay.Combat.Projectiles;
using UnityEngine;

namespace Gameplay.AI.Enemies.Goblin
{
    public class ShamanEnemy : EnemyBase
    {
        private const string ConfigPath = "Enemies/Configs/GoblinShamanConfig";
        private const string AnimationPath = "Enemies/Animations/GoblinShamanAnims";
        private const string ProjectileSpecPath = "Combat/Projectiles/ShamanOrbSpec";
        private const string ProjectilePrefabPath = "Combat/Projectiles/Projectile2D";

        protected override void Awake()
        {
            base.Awake();

            var config = Resources.Load<SimpleEnemyConfig>(ConfigPath);
            if (config == null)
            {
                Debug.LogWarning($"[ShamanEnemy] Missing config at Resources/{ConfigPath}. Using fallback defaults.");
                config = ScriptableObject.CreateInstance<SimpleEnemyConfig>();
                config.Initialize("GoblinShaman", 22f, 7f, 10f, 15f, 4f, 1.8f, 2.5f, Color.white);
            }

            var animationConfig = Resources.Load<AnimationConfig>(AnimationPath);
            if (animationConfig == null)
            {
                Debug.LogWarning($"[ShamanEnemy] Missing animation config at Resources/{AnimationPath}. Create one to enable animations.");
                animationConfig = ScriptableObject.CreateInstance<AnimationConfig>();
                animationConfig.Initialize(null, null, null, null, 10f, 2);
            }

            var projectileSpec = Resources.Load<ProjectileSpec>(ProjectileSpecPath);
            if (projectileSpec == null)
            {
                Debug.LogWarning($"[ShamanEnemy] Missing projectile spec at Resources/{ProjectileSpecPath}. Using fallback defaults.");
                projectileSpec = ScriptableObject.CreateInstance<ProjectileSpec>();
                projectileSpec.Initialize(config.BaseDamage, 6f, 3f, 0.2f);
            }

            var projectilePrefab = Resources.Load<Projectile2D>(ProjectilePrefabPath);
            if (projectilePrefab == null)
            {
                Debug.LogWarning($"[ShamanEnemy] Missing projectile prefab at Resources/{ProjectilePrefabPath}. Using runtime projectile.");
            }

            var health = GetComponent<CharacterHealth>();
            if (health != null)
            {
                health.SetMaxHealth(config.MaxHealth, true);
            }

            var movementController = GetComponent<PlayerMovementController>();
            if (movementController != null)
            {
                movementController.SetBaseMoveSpeed(config.MoveSpeed);
            }

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = config.TintColor;
            }

            var attackStrategy = new RangedProjectileAttackStrategy(
                projectileSpec,
                projectilePrefab,
                Animator,
                animationConfig.AttackTriggerFrame,
                transform,
                config.BaseAttackRange,
                config.AttackCooldown);

            InitializeEnemy(config, animationConfig, attackStrategy, config.BaseAttackRange);
        }

        public override IAIState CreateChaseState()
        {
            return new KeepDistanceState();
        }
    }
}
