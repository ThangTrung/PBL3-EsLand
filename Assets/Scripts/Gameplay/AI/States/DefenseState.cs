using Core.Contracts.AI;
using UnityEngine;

namespace Gameplay.AI.States
{
    /// <summary>
    /// AI State for defensive behavior (blocking, hiding, etc).
    /// Orchestrates animations and delegates logic to an IDefenseStrategy.
    /// </summary>
    public class DefenseState : IAIState
    {
        public void Enter(EnemyBase enemy)
        {
            // Stop moving to focus on defense
            enemy.StopMovement();
            enemy.FaceTarget();
            
            // Delegate to strategy
            if (enemy.DefenseStrategy != null)
            {
                enemy.DefenseStrategy.BeginDefense(enemy);
            }
            else
            {
                // Fallback if no strategy assigned
                enemy.ChangeState(enemy.CreateChaseState());
            }
        }

        public void Execute(EnemyBase enemy)
        {
            if (enemy.DefenseStrategy != null)
            {
                enemy.DefenseStrategy.UpdateDefense(enemy);
                
                // If the defense strategy says it's finished, go back to chasing
                if (!enemy.DefenseStrategy.IsDefending)
                {
                    enemy.ChangeState(enemy.CreateChaseState());
                }
            }
            else
            {
                enemy.ChangeState(enemy.CreateChaseState());
            }
        }

        public void Exit(EnemyBase enemy)
        {
            if (enemy.DefenseStrategy != null)
            {
                enemy.DefenseStrategy.EndDefense(enemy);
            }
        }
    }
}
