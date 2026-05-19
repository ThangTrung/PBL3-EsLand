using Core.Contracts.AI;
using UnityEngine;

namespace Data.Enemies
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Enemies/Simple Enemy Config")]
    public class SimpleEnemyConfig : ScriptableObject, IEnemyConfig
    {
        [SerializeField] private string enemyType = "Enemy";
        [SerializeField] private float maxHealth = 20f;
        [SerializeField] private float baseDamage = 5f;
        [SerializeField] private float baseAttackRange = 2f;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float patrolRadius = 5f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private Color tintColor = Color.white;

        public string EnemyType => enemyType;
        public float MaxHealth => maxHealth;
        public float BaseDamage => baseDamage;
        public float BaseAttackRange => baseAttackRange;
        public float DetectionRange => detectionRange;
        public float PatrolRadius => patrolRadius;
        public float AttackCooldown => attackCooldown;
        public float MoveSpeed => moveSpeed;
        public Color TintColor => tintColor;

        public void Initialize(string type, float health, float damage, float attackRange, float detection, float patrol, float cooldown, float speed, Color tint)
        {
            enemyType = type;
            maxHealth = health;
            baseDamage = damage;
            baseAttackRange = attackRange;
            detectionRange = detection;
            patrolRadius = patrol;
            attackCooldown = cooldown;
            moveSpeed = speed;
            tintColor = tint;
        }
    }
}
