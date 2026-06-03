using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Core.Events;
using UnityEngine.SceneManagement;

namespace UI
{
    public class VictoryPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI victoryText;
        [SerializeField] private Button restartButton;
        
        [Header("Cinematic Settings")]
        [SerializeField] private Image blackBackground;
        [SerializeField] private float fadeDuration = 2f;

        private void Awake()
        {
            if (blackBackground == null && panel != null)
            {
                blackBackground = panel.GetComponent<Image>();
            }

            // Thay vì tắt panel (làm vô hiệu hóa script), chúng ta chỉ làm trong suốt nó
            if (blackBackground != null)
            {
                blackBackground.enabled = false;
            }
            if (victoryText != null)
            {
                victoryText.enabled = false;
            }
            if (restartButton != null)
            {
                restartButton.gameObject.SetActive(false); // Nút bấm là object con nên tắt được
                restartButton.onClick.AddListener(HandleRestart);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnVictory += ShowVictory;
        }

        private void OnDisable()
        {
            GameEvents.OnVictory -= ShowVictory;
        }

        private void ShowVictory()
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
            
            StartCoroutine(VictorySequence());
        }

        private IEnumerator VictorySequence()
        {
            // 1. Chuẩn bị UI
            if (victoryText != null)
            {
                victoryText.enabled = true;
                victoryText.text = "CHIẾN THẮNG!";
                victoryText.color = new Color(1, 1, 1, 0); // Chữ trắng, tàng hình
            }
            
            if (blackBackground != null)
            {
                blackBackground.enabled = true;
                blackBackground.color = new Color(0, 0, 0, 0); // Nền đen, tàng hình
            }

            float elapsed = 0f;

            // 2. Fade in nền đen từ từ
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Dùng unscaled vì timeScale đang = 0
                float alpha = Mathf.Clamp01(elapsed / fadeDuration);
                
                if (blackBackground != null)
                {
                    blackBackground.color = new Color(0, 0, 0, alpha); // Tối dần đến đen xì
                }
                yield return null;
            }

            // 3. Hiện chữ CHIẾN THẮNG trắng to lên
            elapsed = 0f;
            while (elapsed < 1f) // Fade chữ trong 1 giây
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(elapsed / 1f);
                
                if (victoryText != null)
                {
                    victoryText.color = new Color(1, 1, 1, alpha);
                }
                yield return null;
            }

            // 4. Đợi một chút rồi mới hiện nút Chơi lại
            yield return new WaitForSecondsRealtime(1.5f);
            if (restartButton != null)
            {
                restartButton.gameObject.SetActive(true);
            }
        }

        private void HandleRestart()
        {
            Time.timeScale = 1;
            
            // Xóa toàn bộ dữ liệu lưu trữ để chơi lại từ đầu
            if (Infrastructure.SaveSystem.Core.SaveLoadManager.Instance != null)
            {
                Infrastructure.SaveSystem.Core.SaveLoadManager.Instance.DeleteSaveData();
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
