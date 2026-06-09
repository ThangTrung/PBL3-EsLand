using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UI.Guide
{
    /// <summary>
    /// Controller chính quản lý hệ thống Bảng Hướng Dẫn (Guide Panel).
    /// Điều phối việc ẩn/hiện bảng và chuyển đổi giữa các Tab nội dung.
    /// </summary>
    public class GuidePanelController : MonoBehaviour
    {
        [Header("Main UI References")]
        [Tooltip("Nút '?' dùng để bật/tắt Bảng Hướng Dẫn")]
        [SerializeField] private Button helpButton;
        
        [Tooltip("GameObject gốc chứa toàn bộ UI của bảng (Canvas Root hoặc Frame chính)")]
        [SerializeField] private GameObject canvasRoot;

        [Header("Tabs System")]
        [Tooltip("Danh sách các TabButton trong Menu dưới")]
        [SerializeField] private List<TabButton> tabButtons;

        private void Awake()
        {
            // Nếu chưa gán canvasRoot, mặc định lấy chính GameObject này
            if (canvasRoot == null) canvasRoot = gameObject;

            // Gán sự kiện cho nút Help bằng code
            if (helpButton != null)
            {
                helpButton.onClick.AddListener(ToggleUI);
            }
            else
            {
                Debug.LogWarning("[GuidePanelController] Help Button chưa được gán!");
            }

            InitializeTabs();
        }

        private void OnEnable()
        {
            Core.Events.GameEvents.OnExclusiveUIOpened += HandleExclusiveUIOpened;
        }

        private void OnDisable()
        {
            Core.Events.GameEvents.OnExclusiveUIOpened -= HandleExclusiveUIOpened;
        }

        private void HandleExclusiveUIOpened(GameObject source)
        {
            if (canvasRoot != null && source != canvasRoot && canvasRoot.activeSelf)
            {
                SetVisible(false);
            }
        }

        private void Start()
        {
            // Luôn ẩn bảng khi game bắt đầu để đảm bảo trạng thái sạch
            SetVisible(false);
        }

        /// <summary>
        /// Thiết lập sự kiện và trạng thái ban đầu cho các Tab.
        /// </summary>
        private void InitializeTabs()
        {
            if (tabButtons == null || tabButtons.Count == 0)
            {
                Debug.LogWarning("[GuidePanelController] Danh sách TabButtons trống!");
                return;
            }

            foreach (var tab in tabButtons)
            {
                if (tab == null || tab.Button == null) continue;

                // Sử dụng biến cục bộ để tránh lỗi closure (mặc dù C# hiện đại đã xử lý nhưng đây là best practice)
                TabButton currentTab = tab;
                tab.Button.onClick.AddListener(() => SelectTab(currentTab));
            }

            // Mặc định hiển thị nội dung của Tab đầu tiên
            if (tabButtons.Count > 0 && tabButtons[0] != null)
            {
                SelectTab(tabButtons[0]);
            }
        }

        /// <summary>
        /// Kích hoạt Tab được chọn và ẩn tất cả các Tab khác.
        /// </summary>
        /// <param name="selectedTab">Tab vừa được người chơi click</param>
        public void SelectTab(TabButton selectedTab)
        {
            if (selectedTab == null) return;

            foreach (var tab in tabButtons)
            {
                if (tab == null) continue;

                // Chỉ hiện Panel nếu tab đó là tab được chọn
                bool isActive = (tab == selectedTab);
                tab.SetActive(isActive);
            }
        }

        /// <summary>
        /// Đảo ngược trạng thái hiển thị của Panel.
        /// </summary>
        public void ToggleUI()
        {
            if (canvasRoot == null) return;
            SetVisible(!canvasRoot.activeSelf);
        }

        /// <summary>
        /// Điều khiển trực tiếp việc ẩn/hiện Panel.
        /// </summary>
        public void SetVisible(bool isVisible)
        {
            if (canvasRoot != null)
            {
                canvasRoot.SetActive(isVisible);
                if (isVisible)
                {
                    Core.Events.GameEvents.OnExclusiveUIOpened?.Invoke(canvasRoot);
                }
            }
        }
    }
}
