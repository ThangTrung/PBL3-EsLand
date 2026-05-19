using Gameplay.Characters;

namespace Core.Contracts.Combat
{
    public interface IDamageModifier
    {
        /// <summary>
        /// Modifies incoming damage. Return the new damage amount.
        /// Returning 0 can represent a dodge or full block.
        /// </summary>
        float ModifyDamage(float incomingDamage, Character source);
        
        /// <summary>
        /// Priority for processing. Lower values run first.
        /// Useful for: Dodge (0) -> Armor Reduction (10) -> Damage Reflection (20).
        /// </summary>
        int Priority { get; }
    }
}