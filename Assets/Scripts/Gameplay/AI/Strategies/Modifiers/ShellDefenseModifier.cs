using UnityEngine;
using Core.Contracts.Combat;
using Gameplay.Characters;

namespace Gameplay.AI.Strategies.Modifiers
{
    /// <summary>
    /// Giảm sát thương khi rùa rụt cổ (Shell Defense).
    /// </summary>
    public class ShellDefenseModifier : MonoBehaviour, IDamageModifier
    {
        [SerializeField] private float damageReductionMultiplier = 0.5f;
        public int Priority => 10;

        public float ModifyDamage(float damage, Character attacker)
        {
            // Chỉ giảm sát thương nếu là đòn đánh vật lý (ví dụ vậy)
            return damage * damageReductionMultiplier;
        }
    }
}
