using UnityEngine;

namespace Core.Contracts.AI
{
    /// <summary>
    /// Defines how an enemy performs defense actions.
    /// </summary>
    public interface IDefenseStrategy
    {
        /// <summary>
        /// Logic to decide if the enemy should enter defense mode.
        /// </summary>
        bool CanDefend(Gameplay.AI.EnemyBase enemy);

        /// <summary>
        /// Called when the enemy enters the DefenseState.
        /// </summary>
        void BeginDefense(Gameplay.AI.EnemyBase enemy);

        /// <summary>
        /// Called every frame while in DefenseState.
        /// </summary>
        void UpdateDefense(Gameplay.AI.EnemyBase enemy);

        /// <summary>
        /// Called when the enemy exits the DefenseState.
        /// </summary>
        void EndDefense(Gameplay.AI.EnemyBase enemy);
        
        /// <summary>
        /// Returns true if the strategy is currently active.
        /// </summary>
        bool IsDefending { get; }
    }
}
