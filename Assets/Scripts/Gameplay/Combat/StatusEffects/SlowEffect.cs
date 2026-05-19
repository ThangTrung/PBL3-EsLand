using Core.Contracts.Combat;
using UnityEngine;

namespace Gameplay.Combat.StatusEffects
{
    public class SlowEffect : IStatusEffect
    {
        private readonly float _duration;
        private float _elapsed;

        public float SpeedMultiplier { get; }

        public bool IsFinished => _elapsed >= _duration;

        public SlowEffect(float speedMultiplier, float duration)
        {
            SpeedMultiplier = Mathf.Clamp(speedMultiplier, 0.1f, 1f);
            _duration = duration;
        }

        public void OnApply(GameObject target)
        {
        }

        public void Tick(float dt)
        {
            _elapsed += dt;
        }

        public void OnRemove()
        {
        }
    }
}
