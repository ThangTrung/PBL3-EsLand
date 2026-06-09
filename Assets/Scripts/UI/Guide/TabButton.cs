using UnityEngine;
using UnityEngine.UI;

namespace UI.Guide
{
    /// <summary>
    /// Component quản lý một nút Tab trong hệ thống Guide Panel.
    /// Giữ tham chiếu đến nút bấm và Panel nội dung tương ứng.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class TabButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private GameObject contentPanel;

        public Button Button
        {
            get
            {
                if (button == null)
                {
                    button = GetComponent<Button>();
                }
                return button;
            }
        }
        public GameObject ContentPanel => contentPanel;

        private void Awake()
        {
            // Tự động tìm Button nếu chưa được gán
            if (button == null)
            {
                button = GetComponent<Button>();
            }
        }

        /// <summary>
        /// Kích hoạt hoặc hủy kích hoạt Panel nội dung của Tab này.
        /// </summary>
        public void SetActive(bool isActive)
        {
            if (contentPanel != null)
            {
                contentPanel.SetActive(isActive);
            }
        }
    }
}
