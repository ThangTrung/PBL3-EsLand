using Core.Contracts.Combat;
using Gameplay.Characters;
using UnityEngine;

namespace Gameplay.Combat.StatusEffects
{
    public class PoisonEffect : IStatusEffect
    {
        private readonly float _dps;
        private readonly float _duration;
        private readonly Character _source;

        private float _elapsed;
        private float _tickAccumulator;
        private IDamageable _target;

        public bool IsFinished => _elapsed >= _duration;

        public PoisonEffect(float dps, float duration, Character source)
        {
            _dps = dps;
            _duration = duration;
            _source = source;
        }

        public void OnApply(GameObject target)
        {
            _target = target.GetComponent<IDamageable>();
        }

        public void Tick(float dt)
        {
            if (_target == null || IsFinished) return;

            _elapsed += dt;
            _tickAccumulator += dt;

            if (_tickAccumulator >= 1f)
            {
                var ticks = Mathf.FloorToInt(_tickAccumulator);
                _tickAccumulator -= ticks;
                var damage = _dps * ticks;
                _target.TakeDamage(damage, _source);
            }
        }

        public void OnRemove()
        {
        }
    }
}
