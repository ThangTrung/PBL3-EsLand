using UnityEngine;

namespace Core.Contracts.AI
{
    public interface IAttackStrategy
    {
        bool IsAttacking { get; }
        void BeginAttack(Transform target);
        void TryApplyHitIfReady();
        void EndAttack();
        bool CanStartAttack(Transform target);
    }
}
