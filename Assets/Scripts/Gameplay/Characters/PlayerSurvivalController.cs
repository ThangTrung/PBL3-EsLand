using System;
using UnityEngine;

namespace Gameplay.Characters
{
    /// <summary>
    /// Manages survival-related stats for the player character.
    /// Handles Hunger, Thirst, and Stamina logic and drain.
    /// </summary>
    public class PlayerSurvivalController : MonoBehaviour
    {
        [Header("Survival Settings")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float maxThirst = 100f;
        [SerializeField] private float maxStamina = 100f;
        
        [Header("Drain Rates (units/sec)")]
        [SerializeField] private float hungerDrainRate = 5.0f;
        [SerializeField] private float thirstDrainRate = 7.0f;

        [Header("Runtime Stats")]
        [SerializeField] private float currentHunger;
        [SerializeField] private float currentThirst;
        [SerializeField] private float currentStamina;

        public float MaxHunger => maxHunger;
        public float MaxThirst => maxThirst;
        public float MaxStamina => maxStamina;

        public float CurrentHunger => currentHunger;
        public float CurrentThirst => currentThirst;
        public float CurrentStamina => currentStamina;

        public event Action<float> OnHungerChanged;
        public event Action<float> OnThirstChanged;
        // public event Action<float> OnStaminaChanged; // Placeholder for future stamina use

        private void Awake()
        {
            currentHunger = maxHunger;
            currentThirst = maxThirst;
            currentStamina = maxStamina;
        }

        private void Update()
        {
            ConsumeHunger(hungerDrainRate * Time.deltaTime);
            ConsumeThirst(thirstDrainRate * Time.deltaTime);
        }

        public void ConsumeHunger(float amount)
        {
            currentHunger = Mathf.Clamp(currentHunger - amount, 0, maxHunger);
            OnHungerChanged?.Invoke(currentHunger);
        }

        public void ConsumeThirst(float amount)
        {
            currentThirst = Mathf.Clamp(currentThirst - amount, 0, maxThirst);
            OnThirstChanged?.Invoke(currentThirst);
        }

        public void AddHunger(float amount)
        {
            currentHunger = Mathf.Clamp(currentHunger + amount, 0, maxHunger);
            OnHungerChanged?.Invoke(currentHunger);
        }

        public void AddThirst(float amount)
        {
            currentThirst = Mathf.Clamp(currentThirst + amount, 0, maxThirst);
            OnThirstChanged?.Invoke(currentThirst);
        }
    }
}
