using Core.Contracts.Combat;
using UnityEngine;

namespace Gameplay.Combat.StatusEffects
{
    public interface IPigTransformStatus : IStatusEffect
    {
        bool CanAttack { get; }
    }

    public class PigTransformEffect : IPigTransformStatus
    {
        private readonly float _duration;
        private float _elapsedTime;
        private GameObject _target;

        public bool IsFinished => _elapsedTime >= _duration;
        public bool CanAttack => false;
        public float SpeedMultiplier => 0.5f;

        public PigTransformEffect(float duration)
        {
            _duration = duration;
        }

        public void OnApply(GameObject target)
        {
            _target = target;
            _elapsedTime = 0f;
            Debug.Log($"{target.name} turned into a PIG!");
        }

        public void Tick(float deltaTime)
        {
            _elapsedTime += deltaTime;
        }

        public void OnRemove()
        {
            Debug.Log($"{_target.name} is no longer a pig.");
        }
    }
}