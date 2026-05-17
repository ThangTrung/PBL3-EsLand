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

        private void Awake()
        {
            currentHunger = maxHunger;
            currentThirst = maxThirst;
            currentStamina = maxStamina;
        }

        private void Update()
        {
            ModifyHunger(-hungerDrainRate * Time.deltaTime);
            ModifyThirst(-thirstDrainRate * Time.deltaTime);
        }

        public void ModifyHunger(float delta)
        {
            currentHunger = Mathf.Clamp(currentHunger + delta, 0, maxHunger);
            OnHungerChanged?.Invoke(currentHunger);
        }

        public void ModifyThirst(float delta)
        {
            currentThirst = Mathf.Clamp(currentThirst + delta, 0, maxThirst);
            OnThirstChanged?.Invoke(currentThirst);
        }

        public void ConsumeHunger(float amount) => ModifyHunger(-amount);
        public void ConsumeThirst(float amount) => ModifyThirst(-amount);
        public void AddHunger(float amount) => ModifyHunger(amount);
        public void AddThirst(float amount) => ModifyThirst(amount);
    }
}
