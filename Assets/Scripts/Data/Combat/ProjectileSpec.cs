using UnityEngine;

namespace Data.Combat
{
    [CreateAssetMenu(fileName = "ProjectileSpec", menuName = "Combat/Projectile Spec")]
    public class ProjectileSpec : ScriptableObject
    {
        [SerializeField] private float baseDamage = 5f;
        [SerializeField] private float speed = 6f;
        [SerializeField] private float maxLifetime = 3f;
        [SerializeField] private float hitRadius = 0.2f;
        [SerializeField] private bool canPierce = false;

        [Header("Poison")]
        [SerializeField] private bool applyPoison;
        [SerializeField] private float poisonDps = 1f;
        [SerializeField] private float poisonDuration = 3f;

        [Header("Slow")]
        [SerializeField] private bool applySlow;
        [SerializeField] private float slowMultiplier = 0.7f;
        [SerializeField] private float slowDuration = 2f;

        [Header("Visual")]
        [SerializeField] private Sprite projectileSprite;

        public float BaseDamage => baseDamage;
        public float Speed => speed;
        public float MaxLifetime => maxLifetime;
        public float HitRadius => hitRadius;
        public bool CanPierce => canPierce;
        public bool ApplyPoison => applyPoison;
        public float PoisonDps => poisonDps;
        public float PoisonDuration => poisonDuration;
        public bool ApplySlow => applySlow;
        public float SlowMultiplier => slowMultiplier;
        public float SlowDuration => slowDuration;
        public Sprite ProjectileSprite => projectileSprite;

        public void Initialize(float damage, float projectileSpeed, float lifetime, float radius)
        {
            baseDamage = damage;
            speed = projectileSpeed;
            maxLifetime = lifetime;
            hitRadius = radius;
        }
    }
}
