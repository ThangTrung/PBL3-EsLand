using System.Collections.Generic;
using Core.Contracts.Combat;
using UnityEngine;

namespace Gameplay.Combat.StatusEffects
{
    public class StatusEffectController : MonoBehaviour, IStatusEffectReceiver
    {
        private readonly List<IStatusEffect> _effects = new List<IStatusEffect>();

        public bool CanAttack
        {
            get
            {
                foreach (var effect in _effects)
                {
                    if (effect is IPigTransformStatus pig && !pig.CanAttack)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public float SpeedMultiplier
        {
            get
            {
                float multiplier = 1f;
                foreach (var effect in _effects)
                {
                    if (effect is SlowEffect slowEffect)
                    {
                        multiplier = Mathf.Min(multiplier, slowEffect.SpeedMultiplier);
                    }
                }

                return multiplier;
            }
        }

        public void ClearAllEffects()
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                _effects[i].OnRemove();
            }
            _effects.Clear();
        }

        public void ApplyEffect(IStatusEffect effect)
        {
            if (effect == null) return;

            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                if (_effects[i].GetType() == effect.GetType())
                {
                    _effects[i].OnRemove();
                    _effects.RemoveAt(i);
                    break;
                }
            }

            _effects.Add(effect);
            effect.OnApply(gameObject);
        }

        private void Update()
        {
            if (_effects.Count == 0) return;

            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                var effect = _effects[i];
                effect.Tick(Time.deltaTime);
                if (!effect.IsFinished) continue;

                effect.OnRemove();
                _effects.RemoveAt(i);
            }
        }
    }
}
