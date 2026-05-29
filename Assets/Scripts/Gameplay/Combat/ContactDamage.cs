using Core.Contracts.Combat;
using Gameplay.Characters;
using UnityEngine;

namespace Gameplay.Combat
{
    /// <summary>
    /// Deals damage to entities upon physical contact.
    /// Used on enemies to punish the player for touching them.
    /// </summary>
    public class ContactDamage : MonoBehaviour
    {
        [SerializeField] private float damage = 10f;
        [SerializeField] private float damageCooldown = 1.0f;
        [SerializeField] private string targetTag = "Player";

        private float _nextDamageTime;
        private Character _owner;

        private void Awake()
        {
            _owner = GetComponent<Character>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryDealDamage(collision.gameObject);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryDealDamage(collision.gameObject);
        }

        private void TryDealDamage(GameObject target)
        {
            if (Time.time < _nextDamageTime) return;
            if (!target.CompareTag(targetTag)) return;

            if (target.TryGetComponent<IDamageable>(out var health))
            {
                health.TakeDamage(damage, _owner);
                _nextDamageTime = Time.time + damageCooldown;
            }
        }
    }
}
