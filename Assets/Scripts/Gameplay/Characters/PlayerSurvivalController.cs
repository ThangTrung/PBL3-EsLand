using System;
using UnityEngine;
using Data.Survival;

namespace Gameplay.Characters
{
    /// <summary>
    /// Manages survival-related stats for the player character.
    /// Handles Hunger, Thirst, and Stamina logic and drain.
    /// </summary>
    public class PlayerSurvivalController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private SurvivalSettings settings;
        public SurvivalSettings Settings => settings;

        [Header("Runtime Stats")]
        [SerializeField] private float currentHunger;
        [SerializeField] private float currentThirst;
        [SerializeField] private float currentStamina;

        private static readonly int HitHash = Animator.StringToHash("Hit");
        private float _healthLossTimer;
        private CharacterHealth _health;
        private Character _facade;
        private bool _hasHitParameter;

        public float MaxHunger => settings != null ? settings.maxHunger : 100f;
        public float MaxThirst => settings != null ? settings.maxThirst : 100f;
        public float MaxStamina => settings != null ? settings.maxStamina : 100f;

        public float CurrentHunger => currentHunger;
        public float CurrentThirst => currentThirst;
        public float CurrentStamina => currentStamina;

        public bool IsStarving => currentHunger <= 0;
        public bool IsDehydrated => currentThirst <= 0;
        public bool IsHungryCritical => (currentHunger / MaxHunger) < (settings != null ? settings.penaltyThreshold : 0.1f);
        public bool IsThirstyCritical => (currentThirst / MaxThirst) < (settings != null ? settings.penaltyThreshold : 0.1f);
        public bool NeedsPenalty => IsHungryCritical || IsThirstyCritical;

        public event Action<float> OnHungerChanged;
        public event Action<float> OnThirstChanged;
        public event Action<float> OnStaminaChanged;

        private void Awake()
        {
            // Initialize with default values if settings missing (safety)
            currentHunger = MaxHunger;
            currentThirst = MaxThirst;
            currentStamina = MaxStamina;

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
            if (settings == null) return;

            ModifyHunger(-settings.hungerDrainRate * Time.deltaTime);
            ModifyThirst(-settings.thirstDrainRate * Time.deltaTime);
            
            // Hồi phục thể lực theo thời gian nếu chưa đầy
            if (currentStamina < MaxStamina)
            {
                ModifyStamina(settings.staminaRegenRate * Time.deltaTime);
            }

            HandleStarvation();
        }

        private void HandleStarvation()
        {
            if (settings == null) return;

            if (IsStarving || IsDehydrated)
            {
                _healthLossTimer += Time.deltaTime;
                if (_healthLossTimer >= settings.healthLossInterval)
                {
                    _healthLossTimer = 0f;
                    if (_health != null && !_health.IsDead)
                    {
                        // Gây sát thương do đói/khát (không có source)
                        _health.TakeDamage(settings.healthLossAmount, null);
                        
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

        public float GetSpeedMultiplier() => NeedsPenalty ? (settings != null ? settings.slowMultiplier : 0.5f) : 1f;
        public float GetDamageMultiplier() => NeedsPenalty ? (settings != null ? settings.damageMultiplier : 0.5f) : 1f;

        public void ModifyHunger(float delta)
        {
            currentHunger = Mathf.Clamp(currentHunger + delta, 0, MaxHunger);
            OnHungerChanged?.Invoke(currentHunger);
        }

        public void ModifyThirst(float delta)
        {
            currentThirst = Mathf.Clamp(currentThirst + delta, 0, MaxThirst);
            OnThirstChanged?.Invoke(currentThirst);
        }

        public void ModifyStamina(float delta)
        {
            currentStamina = Mathf.Clamp(currentStamina + delta, 0, MaxStamina);
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
