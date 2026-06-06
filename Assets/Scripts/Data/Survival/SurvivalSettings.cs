using UnityEngine;

namespace Data.Survival
{
    [CreateAssetMenu(fileName = "SurvivalSettings", menuName = "Survival/Settings")]
    public class SurvivalSettings : ScriptableObject
    {
        [Header("Max Stats")]
        public float maxHunger = 100f;
        public float maxThirst = 100f;
        public float maxStamina = 100f;
        
        [Header("Drain Rates (units/sec)")]
        public float hungerDrainRate = 1.0f;
        public float thirstDrainRate = 1.2f;
        public float staminaRegenRate = 5.0f;

        [Header("Interaction Restore Amounts")]
        public float thirstRestorePerDrink = 20f;
        public float hungerRestorePerMeat = 30f;

        [Header("Debuff Thresholds")]
        [Range(0, 1)] public float penaltyThreshold = 0.1f; // 10%
        public float slowMultiplier = 0.5f;
        public float damageMultiplier = 0.5f;
        
        [Header("Starvation Damage")]
        public float healthLossInterval = 5f;
        public float healthLossAmount = 1f;
    }
}
