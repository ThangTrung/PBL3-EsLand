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
                    
                    // Load data game từ Cloud ngay sau khi login thành công
                    // SaveLoadManager đã được chỉnh sửa để không tự load ở Start
                    SaveLoadManager.Instance.LoadGame();
                    
                    // Chuyển sang Main Menu (đi qua scene Loading)
                    Invoke(nameof(GoToMainMenu), 0.8f);
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
