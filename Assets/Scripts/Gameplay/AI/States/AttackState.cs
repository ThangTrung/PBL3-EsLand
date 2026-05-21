using Core.Contracts.AI;
using UnityEngine;

namespace Gameplay.AI.States
{
    public class AttackState : IAIState
    {
        private float _attackDuration;
        private float _timer;
        private bool _attackFinished;

        public void Enter(EnemyBase enemy)
        {
            enemy.StopMovement();
            enemy.FaceTarget();
            
            _timer = 0f;
            _attackFinished = false;
            
            _attackDuration = 0.5f;
            if (enemy.Animator != null && enemy.Animator.Config != null && enemy.Animator.Config.AttackFrames != null && enemy.Animator.Config.FrameRate > 0)
            {
                _attackDuration = (float)enemy.Animator.Config.AttackFrames.Length / enemy.Animator.Config.FrameRate;
            }

            enemy.AttackStrategy?.BeginAttack(enemy.Target);
        }

        public void Execute(EnemyBase enemy)
        {
            if (enemy.Target == null)
            {
                enemy.ChangeState(new PatrolState());
                return;
            }

            enemy.AttackStrategy?.TryApplyHitIfReady();

            _timer += Time.deltaTime;
            bool animFinished = enemy.Animator != null && enemy.Animator.IsCurrentAnimationFinished();

            if (_timer >= _attackDuration || animFinished)
            {
                if (!_attackFinished)
                {
                    enemy.AttackStrategy?.EndAttack();
                    _attackFinished = true;
                }

                if (enemy.AttackStrategy != null && enemy.AttackStrategy.CanStartAttack(enemy.Target))
                {
                    enemy.AttackStrategy.BeginAttack(enemy.Target);
                    _timer = 0f;
                    _attackFinished = false;
                    _attackDuration = 0.5f;
                    if (enemy.Animator != null && enemy.Animator.Config != null && enemy.Animator.Config.AttackFrames != null && enemy.Animator.Config.FrameRate > 0)
                    {
                        _attackDuration = (float)enemy.Animator.Config.AttackFrames.Length / enemy.Animator.Config.FrameRate;
                    }
                    return;
                }

                enemy.ChangeState(enemy.CreateChaseState());
            }
        }

        public void Exit(EnemyBase enemy)
        {
            if (!_attackFinished)
            {
                enemy.AttackStrategy?.EndAttack();
            }
        }
    }
}
