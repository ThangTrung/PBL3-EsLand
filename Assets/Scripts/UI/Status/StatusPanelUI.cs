using Gameplay.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Status
{
    /// <summary>
    /// Quản lý giao diện các thanh trạng thái (Máu, Đói, Khát, Thể lực).
    /// Sử dụng kỹ thuật Lerp để cập nhật mượt mà.
    /// </summary>
    public class StatusPanelUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image healthImage;
        [SerializeField] private Image hungerImage;
        [SerializeField] private Image thirstImage;
        [SerializeField] private Image staminaImage;

        private CharacterHealth _playerHealth;
        private PlayerSurvivalController _playerSurvival;

        [Header("Smooth Settings")]
        [SerializeField] private float lerpSpeed = 5f;

        private float _targetHealth;
        private float _targetHunger;
        private float _targetThirst;
        private float _targetStamina;

        public void Initialize(Player player)
        {
            if (player == null)
            {
                return;
            }

            // Health
            _playerHealth = player.GetComponent<CharacterHealth>();
            if (_playerHealth != null)
            {
                _targetHealth = _playerHealth.CurrentHealth / _playerHealth.MaxHealth;
                if (healthImage != null) healthImage.fillAmount = _targetHealth;
                _playerHealth.OnHealthChanged += HandleHealthChanged;
            }

            // Survival Stats
            _playerSurvival = player.GetComponent<PlayerSurvivalController>();
            if (_playerSurvival != null)
            {
                _targetHunger = _playerSurvival.CurrentHunger / _playerSurvival.MaxHunger;
                if (hungerImage != null) hungerImage.fillAmount = _targetHunger;
                _playerSurvival.OnHungerChanged += HandleHungerChanged;

                _targetThirst = _playerSurvival.CurrentThirst / _playerSurvival.MaxThirst;
                if (thirstImage != null) thirstImage.fillAmount = _targetThirst;
                _playerSurvival.OnThirstChanged += HandleThirstChanged;

                _targetStamina = _playerSurvival.CurrentStamina / _playerSurvival.MaxStamina;
                if (staminaImage != null) staminaImage.fillAmount = _targetStamina;
                _playerSurvival.OnStaminaChanged += HandleStaminaChanged;
            }
        }

        private void HandleHealthChanged(float val) => _targetHealth = val / _playerHealth.MaxHealth;
        private void HandleHungerChanged(float val) => _targetHunger = val / _playerSurvival.MaxHunger;
        private void HandleThirstChanged(float val) => _targetThirst = val / _playerSurvival.MaxThirst;
        private void HandleStaminaChanged(float val) 
        {
            _targetStamina = val / _playerSurvival.MaxStamina;
        }

        private void Update()
        {
            UpdateBarsSmoothly();
        }

        private void UpdateBarsSmoothly()
        {
            if (healthImage != null)
                healthImage.fillAmount = Mathf.Lerp(healthImage.fillAmount, _targetHealth, Time.deltaTime * lerpSpeed);
            
            if (hungerImage != null)
                hungerImage.fillAmount = Mathf.Lerp(hungerImage.fillAmount, _targetHunger, Time.deltaTime * lerpSpeed);
            
            if (thirstImage != null)
                thirstImage.fillAmount = Mathf.Lerp(thirstImage.fillAmount, _targetThirst, Time.deltaTime * lerpSpeed);
            
            if (staminaImage != null)
            {
                staminaImage.fillAmount = Mathf.Lerp(staminaImage.fillAmount, _targetStamina, Time.deltaTime * lerpSpeed);
            }
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged -= HandleHealthChanged;
            }

            if (_playerSurvival != null)
            {
                _playerSurvival.OnHungerChanged -= HandleHungerChanged;
                _playerSurvival.OnThirstChanged -= HandleThirstChanged;
                _playerSurvival.OnStaminaChanged -= HandleStaminaChanged;
            }
        }
    }
}
