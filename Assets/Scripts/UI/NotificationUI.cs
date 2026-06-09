using UnityEngine;
using TMPro;
using Core.Events;
using System.Collections;

namespace UI
{
    /// <summary>
    /// Hiển thị thông báo trên màn hình (ví dụ: Thiếu điều kiện mở Boss).
    /// </summary>
    public class NotificationUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private GameObject notificationPanel;

        [Header("Settings")]
        [SerializeField] private float displayDuration = 3f;

        private Coroutine _hideCoroutine;

        private void OnEnable()
        {
            GameEvents.OnShowNotification += ShowNotification;
            
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
        }

        private void OnDisable()
        {
            GameEvents.OnShowNotification -= ShowNotification;
        }

        private void ShowNotification(string message)
        {
            if (notificationText == null || notificationPanel == null) return;

            notificationText.text = message;
            notificationPanel.SetActive(true);

            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
            }
            _hideCoroutine = StartCoroutine(HideNotificationRoutine());
        }

        private IEnumerator HideNotificationRoutine()
        {
            yield return new WaitForSeconds(displayDuration);
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
        }
    }
}
