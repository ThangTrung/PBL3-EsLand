using Gameplay.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Status
{
    public class StatusPanelUI : MonoBehaviour
    {
        [Header("UI Elements")]
                public Image healthImage;
        public Image hungerImage;
        public Image thirstImage;
        public Image staminaImage;

        private CharacterHealth _playerHealth;

        public void Initialize(Player player)
        {
            if (player == null) return;

            // Health
            _playerHealth = player.GetComponent<CharacterHealth>();
            if (_playerHealth != null)
            {
                UpdateHealthUI(_playerHealth.CurrentHealth);
                _playerHealth.OnHealthChanged += UpdateHealthUI;
            }

            // Hunger
            UpdateHungerUI(player.CurrentHunger, player.MaxHunger);
            player.OnHungerChanged += (val) => UpdateHungerUI(val, player.MaxHunger);

            // Thirst
            UpdateThirstUI(player.CurrentThirst, player.MaxThirst);
            player.OnThirstChanged += (val) => UpdateThirstUI(val, player.MaxThirst);

            // Stamina
            UpdateStaminaUI(player.CurrentStamina, player.MaxStamina);
            player.OnStaminaChanged += (val) => UpdateStaminaUI(val, player.MaxStamina);
        }

        private void OnDisable()
        {
            if (_playerHealth != null)
            {
                _playerHealth.OnHealthChanged -= UpdateHealthUI;
            }
        }

        private void UpdateHealthUI(float currentHealth)
        {
            if (healthImage != null && _playerHealth != null)
            {
                healthImage.fillAmount = currentHealth / _playerHealth.MaxHealth;
            }
        }

        private void UpdateHungerUI(float current, float max)
        {
            if (hungerImage != null) hungerImage.fillAmount = current / max;
        }

        private void UpdateThirstUI(float current, float max)
        {
            if (thirstImage != null) thirstImage.fillAmount = current / max;
        }

        private void UpdateStaminaUI(float current, float max)
        {
            if (staminaImage != null) staminaImage.fillAmount = current / max;
        }

    }
}
