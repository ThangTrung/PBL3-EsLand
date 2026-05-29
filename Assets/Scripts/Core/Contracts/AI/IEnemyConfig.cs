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
        
        // Defense Configuration
        float DefenseCooldown { get; }
        float DefenseDuration { get; }
        float DefenseChance { get; }
        
        float VerticalAlignmentOffset { get; }
        float HorizontalAlignmentOffset { get; }
        float MoveSpeed { get; }
        Color TintColor { get; }
        string LootItemId { get; }
        int LootQuantity { get; }

    }
}