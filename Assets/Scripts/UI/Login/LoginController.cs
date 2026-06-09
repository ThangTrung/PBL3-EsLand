using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Infrastructure.SaveSystem.Core;
using Core.SceneManagement;

namespace UI.Login
{
    /// <summary>
    /// Điều khiển UI của màn hình Đăng nhập.
    /// </summary>
    public class LoginController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private TextMeshProUGUI statusText;

        private void Start()
        {
            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginClicked);
            }
            
            if (statusText != null) statusText.text = "Chào mừng bạn đến với EsLand!";
        }

        private void OnLoginClicked()
        {
            string user = usernameInput.text;
            string pass = passwordInput.text;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                ShowStatus("Vui lòng nhập Username và Password", Color.yellow);
                return;
            }

            loginButton.interactable = false;
            ShowStatus("Đang kết nối Server...", Color.white);

            StartCoroutine(CloudAuthService.Login(user, pass, (success, message) => {
                if (success)
                {
                    ShowStatus(message, Color.green);
                    
                    // Thiết lập user ID và cấu hình Cloud
                    string serverIp = Infrastructure.Networking.NetworkSettings.Instance != null 
                        ? Infrastructure.Networking.NetworkSettings.Instance.ServerIp 
                        : "localhost";

                    SaveLoadManager.Instance.EnableCloudMode(CloudAuthService.CurrentUserID, serverIp, (cloudSuccess, cloudMsg) => {
                        if (cloudSuccess)
                        {
                            Debug.Log($"[LoginController] Cloud connected. {cloudMsg}");
                        }
                        else
                        {
                            Debug.LogWarning($"[LoginController] Cloud load warning: {cloudMsg}");
                        }
                        
                        // Chuyển sang Main Menu sau khi đã setup xong Cloud (đi qua scene Loading)
                        GoToMainMenu();
                    });
                }
                else
                {
                    loginButton.interactable = true;
                    ShowStatus(message, Color.red);
                }
            }));
        }

        private void GoToMainMenu()
        {
            SceneLoader.Load("MainMenu");
        }

        private void ShowStatus(string msg, Color color)
        {
            if (statusText != null)
            {
                statusText.text = msg;
                statusText.color = color;
            }
        }
    }
}
