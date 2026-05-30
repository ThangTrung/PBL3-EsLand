using Core.Contracts.Combat;
using Gameplay.Characters;
using UnityEngine;

namespace Gameplay.AI.Strategies.Modifiers
{
    /// <summary>
    /// Modifier that reduces incoming damage when active.
    /// Used during defense animations (Blocking, Shielding, Hiding in shell).
    /// </summary>
    public class BlockDamageModifier : MonoBehaviour, IDamageModifier
    {
        [SerializeField, Range(0, 1)] private float damageMultiplier = 0.2f; // Default 80% reduction
        
        public int Priority => 10; // Process after Dodge (0)
        
        public bool IsActive { get; set; }

        public float ModifyDamage(float incomingDamage, Character source)
        {
            if (IsActive)
            {
                float reducedDamage = incomingDamage * damageMultiplier;
                // Debug.Log($"[Block] Damage reduced from {incomingDamage} to {reducedDamage}");
                return reducedDamage;
            }
            
            return incomingDamage;
        }

        public void SetMultiplier(float multiplier) => damageMultiplier = Mathf.Clamp01(multiplier);
    }
}
