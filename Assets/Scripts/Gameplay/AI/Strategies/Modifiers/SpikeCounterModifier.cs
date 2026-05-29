using Core.Contracts.Combat;
using Gameplay.Characters;
using UnityEngine;

namespace Gameplay.AI.Strategies.Modifiers
{
    public class SpikeCounterModifier : MonoBehaviour, IDamageModifier
    {
        [SerializeField] private float reflectionPercent = 0.25f;
        [SerializeField] private float reflectionMaxRange = 3f;

        public int Priority => 20; // Runs after reduction

        public float ModifyDamage(float incomingDamage, Character source)
        {
            if (source != null && incomingDamage > 0)
            {
                float distance = Vector3.Distance(transform.position, source.transform.position);
                if (distance <= reflectionMaxRange)
                {
                    float reflectedDamage = incomingDamage * reflectionPercent;
                    if (source.TryGetComponent<IDamageable>(out var victim))
                    {
                        // Pass null as source to prevent infinite recursion
                        victim.TakeDamage(reflectedDamage, null);
                    }
                }
            }
            return incomingDamage;
        }
    }
}
