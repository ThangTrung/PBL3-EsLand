using Core.Contracts.Combat;
using Gameplay.AI;
using Gameplay.Characters;
using UnityEngine;

namespace Gameplay.AI.Strategies.Modifiers
{
    public class ShellDefenseModifier : MonoBehaviour, IDamageModifier
    {
        [SerializeField] private float healthThreshold = 0.5f;
        [SerializeField] private float damageReduction = 0.7f;
        [SerializeField] private float duration = 6f;
        [SerializeField] private float cooldown = 8f;

        private CharacterHealth _health;
        private EnemyBase _enemy;
        private float _lastTriggerTime = -999f;
        private bool _isInShellMode;

        public int Priority => 10; // Reduction runs after dodge but before reflection

        private void Awake()
        {
            _health = GetComponent<CharacterHealth>();
            _enemy = GetComponent<EnemyBase>();
        }

        public float ModifyDamage(float incomingDamage, Character source)
        {
            if (_isInShellMode)
            {
                return incomingDamage * (1f - damageReduction);
            }

            if (_health != null && _health.CurrentHealth / _health.MaxHealth <= healthThreshold)
            {
                if (Time.time - _lastTriggerTime >= cooldown + duration)
                {
                    EnterShellMode();
                }
            }

            return incomingDamage;
        }

        private void EnterShellMode()
        {
            _isInShellMode = true;
            _lastTriggerTime = Time.time;
            _enemy?.StopMovement();
            // Trigger animation state if handled by animator
            Debug.Log($"{gameObject.name} entered Shell Mode!");
            Invoke(nameof(ExitShellMode), duration);
        }

        private void ExitShellMode()
        {
            _isInShellMode = false;
            Debug.Log($"{gameObject.name} exited Shell Mode.");
        }
    }
}