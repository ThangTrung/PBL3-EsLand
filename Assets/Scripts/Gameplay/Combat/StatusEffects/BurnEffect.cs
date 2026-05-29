using Core.Contracts.Combat;
using Gameplay.Characters;
using UnityEngine;

namespace Gameplay.Combat.StatusEffects
{
    public class BurnEffect : IStatusEffect
    {
        private readonly float _damagePerSecond;
        private readonly float _duration;
        private readonly Character _source;
        private float _elapsedTime;
        private float _tickTimer;
        private GameObject _target;
        private IDamageable _damageable;

        public bool IsFinished => _elapsedTime >= _duration;

        public BurnEffect(float dps, float duration, Character source)
        {
            _damagePerSecond = dps;
            _duration = duration;
            _source = source;
        }

        public void OnApply(GameObject target)
        {
            _target = target;
            _damageable = target.GetComponent<IDamageable>();
            _elapsedTime = 0f;
            _tickTimer = 0f;
        }

        public void Tick(float deltaTime)
        {
            _elapsedTime += deltaTime;
            _tickTimer += deltaTime;

            if (_tickTimer >= 1f)
            {
                _tickTimer -= 1f;
                _damageable?.TakeDamage(_damagePerSecond, _source);
            }
        }

        public void OnRemove()
        {
        }
    }
}
