using Core.Contracts.AI;
using Gameplay.AI.Movement;
using UnityEngine;

namespace Gameplay.AI.Strategies
{
    public class PassiveFleeStrategy : IAttackStrategy
    {
        private readonly EnemyMovementController _movement;
        private readonly Transform _self;
        private readonly float _fleeDistance = 5f;

        public bool IsAttacking => false;

        public PassiveFleeStrategy(EnemyMovementController movement, Transform self)
        {
            _movement = movement;
            _self = self;
        }

        public void BeginAttack(Transform target)
        {
            Flee(target);
        }

        public void TryApplyHitIfReady() { }

        public void EndAttack() { }

        public bool CanStartAttack(Transform target)
        {
            return target != null && Vector3.Distance(_self.position, target.position) < 3f;
        }

        private void Flee(Transform target)
        {
            if (_movement == null || target == null) return;
            Vector3 fleeDir = (_self.position - target.position).normalized;
            _movement.Move(fleeDir * _fleeDistance);
        }
    }
}
