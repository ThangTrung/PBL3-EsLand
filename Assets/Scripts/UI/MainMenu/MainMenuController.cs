using UnityEngine;
using UnityEngine.UI;
using Infrastructure.SaveSystem.Core;
using Core.SceneManagement;

namespace UI.MainMenu
{
    /// <summary>
    /// Điều khiển UI của màn hình Menu chính.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;

        private void Start()
        {
            // Kiểm tra xem đã có dữ liệu game chưa (load từ cloud ở bước Login)
            bool hasData = false;
            if (SaveLoadManager.Instance != null)
            {
                hasData = SaveLoadManager.Instance.HasData();
            }
            
            // Thiết lập trạng thái nút Continue
            if (continueButton != null)
            {
                // Nếu chưa có data thì ẩn hoặc làm mờ nút Continue
                continueButton.interactable = hasData;
                
                // Thêm hiệu ứng hình ảnh nếu cần (ví dụ: alpha)
                var img = continueButton.GetComponent<Image>();
                if (img != null)
                {
                    var color = img.color;
                    color.a = hasData ? 1f : 0.5f;
                    img.color = color;
                }
            }

            // Gán sự kiện cho các nút
            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(OnNewGameClicked);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        private void OnNewGameClicked()
        {
            Debug.Log("[MainMenu] Starting New Game...");
            // Khởi tạo data mới
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.NewGame();
            }
            
            // Chuyển vào scene chơi game
            SceneLoader.Load("Map2Kt");
        }

        private void OnContinueClicked()
        {
            Debug.Log("[MainMenu] Continuing Game...");
            // Load vào scene chơi game (Data đã được load sẵn trong SaveLoadManager)
            SceneLoader.Load("Map2Kt");
        }
    }
}
