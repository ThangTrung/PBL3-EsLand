using Core.Contracts.Combat;
using Gameplay.Characters;
using UnityEngine;

namespace Gameplay.AI.Strategies.Modifiers
{
    public class DodgeModifier : MonoBehaviour, IDamageModifier
    {
        [SerializeField, Range(0, 1)] private float dodgeChance = 0.2f;
        
        public int Priority => 0; // Dodge runs first

        public float ModifyDamage(float incomingDamage, Character source)
        {
            if (Random.value <= dodgeChance)
            {
                Debug.Log($"{gameObject.name} dodged the attack!");
                // Trigger dodge animation if available
                return 0f;
            }
            return incomingDamage;
        }

        public void SetDodgeChance(float chance) => dodgeChance = chance;
    }
}