using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Hệ thống Tooltip tập trung cho toàn bộ game.
    /// Giúp tiết kiệm tài nguyên bằng cách chỉ dùng 1 bảng hiển thị duy nhất.
    /// </summary>
    public class GlobalTooltipUI : MonoBehaviour
    {
        public static GlobalTooltipUI Instance { get; private set; }

        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Vector2 offset = new Vector2(15, -15);

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            Hide();
        }

        private void Update()
        {
            if (tooltipPanel != null && tooltipPanel.activeSelf)
            {
                FollowMouse();
            }
        }

        public void Show(string title, string content)
        {
            if (tooltipPanel == null) return;

            tooltipPanel.SetActive(true);
            if (nameText) nameText.text = title;
            if (descriptionText) descriptionText.text = content;
            
            FollowMouse();
        }

        public void Hide()
        {
            if (tooltipPanel) tooltipPanel.SetActive(false);
        }

        private void FollowMouse()
        {
            Vector2 mousePos = Input.mousePosition;
            rectTransform.position = mousePos + offset;

            // Đảm bảo tooltip không bị tràn khỏi màn hình (Pivot adjustment)
            float canvasWidth = Screen.width;
            float canvasHeight = Screen.height;
            
            float pivotX = mousePos.x > canvasWidth * 0.7f ? 1 : 0;
            float pivotY = mousePos.y < canvasHeight * 0.3f ? 0 : 1;
            
            rectTransform.pivot = new Vector2(pivotX, pivotY);
        }
    }
}
