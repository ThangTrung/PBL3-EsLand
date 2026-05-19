using Core.Events;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Gameplay.UI
{
    public class BossHealthBarUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI bossNameText;
        [SerializeField] private GameObject container; // The root object of the health bar (to show/hide)

        private void Awake()
        {
            if (container != null)
            {
                container.SetActive(false);
            }
        }

        private void OnEnable()
        {
            // Subscribe to Boss Events
            BossHealthEventChannel.OnBossHealthUpdated += HandleBossHealthUpdated;
            BossHealthEventChannel.OnBossDefeated += HandleBossDefeated;
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks
            BossHealthEventChannel.OnBossHealthUpdated -= HandleBossHealthUpdated;
            BossHealthEventChannel.OnBossDefeated -= HandleBossDefeated;
        }

        private void HandleBossHealthUpdated(string bossName, float currentHP, float maxHP)
        {
            if (container != null && !container.activeSelf)
            {
                container.SetActive(true);
            }

            if (bossNameText != null)
            {
                bossNameText.text = bossName;
            }

            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHP;
                healthSlider.value = currentHP;
            }
        }

        private void HandleBossDefeated()
        {
            if (container != null)
            {
                container.SetActive(false);
            }
        }
    }
}