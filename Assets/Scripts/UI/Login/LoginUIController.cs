using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Infrastructure.SaveSystem.Core;
using UnityEngine.SceneManagement;

namespace UI.Login
{
    /// <summary>
    /// LoginUIController - Điều phối luồng đăng nhập và cấu hình mạng cho Hybrid Save.
    /// </summary>
    public class LoginUIController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField serverIPInput;
        [SerializeField] private TMP_InputField userIDInput;
        [SerializeField] private Toggle cloudModeToggle;
        [SerializeField] private Button connectButton;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Scene Settings")]
        [SerializeField] private string gameplaySceneName = "Map2Kt";

        private void Start()
        {
            if (connectButton != null)
                connectButton.onClick.AddListener(OnConnectButtonClicked);

            // Gợi ý IP mặc định (localhost) cho demo nhanh
            if (serverIPInput != null && string.IsNullOrEmpty(serverIPInput.text))
                serverIPInput.text = "localhost";
                
            UpdateStatus("Sẵn sàng kết nối.", Color.white);
        }

        public void OnConnectButtonClicked()
        {
            string ip = serverIPInput != null ? serverIPInput.text : "localhost";
            string id = userIDInput != null ? userIDInput.text : "DemoUser";
            bool isCloud = cloudModeToggle != null && cloudModeToggle.isOn;

            if (string.IsNullOrEmpty(id))
            {
                UpdateStatus("Vui lòng nhập User ID!", Color.yellow);
                return;
            }

            UpdateStatus("Đang cấu hình hệ thống...", Color.cyan);

            // 1. Cấu hình cho SaveLoadManager
            if (SaveLoadManager.Instance != null)
            {
                if (isCloud)
                {
                    UpdateStatus("Đang kiểm tra kết nối Cloud...", Color.cyan);
                    // Kích hoạt Cloud Mode và thử Load data ngay tại đây
                    SaveLoadManager.Instance.EnableCloudMode(id, ip, (success, message) => {
                        if (success)
                        {
                            Debug.Log($"[LoginUI] Cloud connected. {message}");
                            EnterGame();
                        }
                        else
                        {
                            UpdateStatus($"Lỗi Cloud: {message}. Vẫn vào game chế độ Local.", Color.yellow);
                            // Vẫn cho vào game nếu lỗi cloud (Fallback mechanism)
                            Invoke(nameof(EnterGame), 1.5f);
                        }
                    });
                }
                else
                {
                    // Chế độ Local thuần túy
                    SaveLoadManager.Instance.EnableCloudMode(null, null); // Hoặc một hàm DisableCloudMode nếu có
                    EnterGame();
                }
            }
            else
            {
                UpdateStatus("Lỗi: Không tìm thấy SaveLoadManager!", Color.red);
            }
        }

        private void EnterGame()
        {
            UpdateStatus("Đang vào game...", Color.green);
            SceneManager.LoadScene(gameplaySceneName);
        }

        private void UpdateStatus(string msg, Color color)
        {
            if (statusText != null)
            {
                statusText.text = msg;
                statusText.color = color;
            }
        }
    }
}
