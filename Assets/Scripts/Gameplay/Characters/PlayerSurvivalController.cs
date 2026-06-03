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
        [SerializeField] private float staminaRegenRate = 5.0f;

        [Header("Debuff Settings")]
        [SerializeField] private float thresholdPercentage = 0.1f; // 10%
        [SerializeField] private float slowMultiplier = 0.5f;
        [SerializeField] private float damageMultiplier = 0.5f;
        [SerializeField] private float healthLossInterval = 5f;
        [SerializeField] private float healthLossAmount = 1f;

        [Header("Runtime Stats")]
        [SerializeField] private float currentHunger;
        [SerializeField] private float currentThirst;
        [SerializeField] private float currentStamina;

        private static readonly int HitHash = Animator.StringToHash("Hit");
        private float _healthLossTimer;
        private CharacterHealth _health;
        private Character _facade;
        private bool _hasHitParameter;

        public float MaxHunger => maxHunger;
        public float MaxThirst => maxThirst;
        public float MaxStamina => maxStamina;

        public float CurrentHunger => currentHunger;
        public float CurrentThirst => currentThirst;
        public float CurrentStamina => currentStamina;

        public bool IsStarving => currentHunger <= 0;
        public bool IsDehydrated => currentThirst <= 0;
        public bool IsHungryCritical => (currentHunger / maxHunger) < thresholdPercentage;
        public bool IsThirstyCritical => (currentThirst / maxThirst) < thresholdPercentage;
        public bool NeedsPenalty => IsHungryCritical || IsThirstyCritical;

        public event Action<float> OnHungerChanged;
        public event Action<float> OnThirstChanged;
        public event Action<float> OnStaminaChanged;

        private void Awake()
        {
            currentHunger = maxHunger;
            currentThirst = maxThirst;
            currentStamina = maxStamina;

            _health = GetComponent<CharacterHealth>();
            _facade = GetComponent<Character>();
        }

        private void Start()
        {
            if (_facade != null && _facade.Animator != null)
                _hasHitParameter = HasParameter(_facade.Animator, HitHash);
        }

        private void Update()
        {
            ModifyHunger(-hungerDrainRate * Time.deltaTime);
            ModifyThirst(-thirstDrainRate * Time.deltaTime);
            
            // Hồi phục thể lực theo thời gian nếu chưa đầy
            if (currentStamina < maxStamina)
            {
                ModifyStamina(staminaRegenRate * Time.deltaTime);
            }

            HandleStarvation();
        }

        private void HandleStarvation()
        {
            if (IsStarving || IsDehydrated)
            {
                _healthLossTimer += Time.deltaTime;
                if (_healthLossTimer >= healthLossInterval)
                {
                    _healthLossTimer = 0f;
                    if (_health != null && !_health.IsDead)
                    {
                        // Gây sát thương do đói/khát (không có source)
                        _health.TakeDamage(healthLossAmount, null);
                        
                        // Kích hoạt hoạt ảnh bị thương nếu có
                        if (_facade != null && _facade.Animator != null && _hasHitParameter)
                        {
                            _facade.Animator.SetTrigger(HitHash);
                        }
                    }
                }
            }
            else
            {
                _healthLossTimer = 0f;
            }
        }

        private bool HasParameter(Animator animator, int paramHash)
        {
            if (animator == null) return false;
            foreach (var param in animator.parameters)
                if (param.nameHash == paramHash) return true;
            return false;
        }

        public float GetSpeedMultiplier() => NeedsPenalty ? slowMultiplier : 1f;
        public float GetDamageMultiplier() => NeedsPenalty ? damageMultiplier : 1f;

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

        public void ModifyStamina(float delta)
        {
            currentStamina = Mathf.Clamp(currentStamina + delta, 0, maxStamina);
            OnStaminaChanged?.Invoke(currentStamina);
        }

        public void ConsumeHunger(float amount) => ModifyHunger(-amount);
        public void ConsumeThirst(float amount) => ModifyThirst(-amount);
        public void AddHunger(float amount) => ModifyHunger(amount);
        public void AddThirst(float amount) => ModifyThirst(amount);
        
        public bool HasEnoughStamina(float amount) => currentStamina >= amount;
        
        public bool TryConsumeStamina(float amount)
        {
            if (!HasEnoughStamina(amount)) return false;
            ModifyStamina(-amount);
            return true;
        }
    }
}
