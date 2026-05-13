using Gameplay.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Status
{
    public class StatusPanelUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image healthImage;

        private CharacterHealth _playerHealth;

        public void Initialize(Player player)
        {
            if (player == null) return;

            _playerHealth = player.GetComponent<CharacterHealth>();
            if (_playerHealth != null)
            {
                // Set initial value
                if (healthImage != null)
                {
                    healthImage.fillAmount = _playerHealth.CurrentHealth / _playerHealth.MaxHealth;
                }

                // Lắng nghe sự thay đổi máu
                _playerHealth.OnHealthChanged += UpdateHealthUI;
            }
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
    }
}
