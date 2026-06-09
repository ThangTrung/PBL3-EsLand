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

        private bool _isWaitingForRegister = false;
        private string _pendingUser;
        private string _pendingPass;

        private void Start()
        {
            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginClicked);
            }
            
            if (statusText != null) statusText.text = "Chào mừng bạn đến với EsLand!";
        }

        private void Update()
        {
            // Kiểm tra phím Y/N khi đang ở trạng thái chờ xác nhận đăng ký
            if (_isWaitingForRegister)
            {
                if (Input.GetKeyDown(KeyCode.Y))
                {
                    ConfirmRegister();
                }
                else if (Input.GetKeyDown(KeyCode.N))
                {
                    CancelRegister();
                }
            }
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

            _pendingUser = user;
            _pendingPass = pass;
            
            loginButton.interactable = false;
            ShowStatus("Đang kết nối Server...", Color.white);

            StartCoroutine(CloudAuthService.Login(user, pass, (success, message) => {
                if (success)
                {
                    HandleLoginSuccess(message);
                }
                else
                {
                    if (message.Contains("không tồn tại"))
                    {
                        _isWaitingForRegister = true;
                        ShowStatus(message + " Bạn có muốn tạo mới không? (Bấm Y: Có, N: Không)", Color.cyan);
                    }
                    else
                    {
                        loginButton.interactable = true;
                        ShowStatus(message, Color.red);
                    }
                }
            }));
        }

        private void ConfirmRegister()
        {
            _isWaitingForRegister = false;
            ShowStatus("Đang tạo tài khoản mới...", Color.white);
            
            StartCoroutine(CloudAuthService.Register(_pendingUser, _pendingPass, (success, message) => {
                if (success)
                {
                    HandleLoginSuccess("Đăng ký thành công! " + message);
                }
                else
                {
                    loginButton.interactable = true;
                    ShowStatus("Lỗi đăng ký: " + message, Color.red);
                }
            }));
        }

        private void CancelRegister()
        {
            _isWaitingForRegister = false;
            loginButton.interactable = true;
            ShowStatus("Đã hủy. Vui lòng nhập lại tài khoản khác.", Color.yellow);
        }

        private void HandleLoginSuccess(string message)
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
