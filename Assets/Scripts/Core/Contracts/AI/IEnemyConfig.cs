using UnityEngine;

using UnityEngine;

namespace Core.Contracts.AI
{
    public interface IEnemyConfig
    {
        string EnemyType { get; }
        float MaxHealth { get; }
        float BaseDamage { get; }
        float BaseAttackRange { get; }
        float DetectionRange { get; }
        float PatrolRadius { get; }
        float AttackCooldown { get; }
        float MoveSpeed { get; }
        Color TintColor { get; }
    }
}
