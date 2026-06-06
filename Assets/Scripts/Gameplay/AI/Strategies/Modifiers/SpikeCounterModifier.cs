using UnityEngine;
using Core.Contracts.Combat;
using Gameplay.Characters;

namespace Gameplay.AI.Strategies.Modifiers
{
    /// <summary>
    /// Phản sát thương khi bị tấn công (Spike Counter).
    /// </summary>
    public class SpikeCounterModifier : MonoBehaviour, IDamageModifier
    {
        [SerializeField] private float counterDamageMultiplier = 0.2f;
        public int Priority => 20;

        public float ModifyDamage(float damage, Character attacker)
        {
            if (attacker != null && attacker.Health != null)
            {
                attacker.Health.TakeDamage(damage * counterDamageMultiplier, GetComponent<Character>());
            }
            return damage;
        }
    }
}
