using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace UI.Settings
{
    public class SettingsButtonController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button mainButton;
        [SerializeField] private SettingsMenuUI menuUI;
        [SerializeField] private SettingsActionHandler actionHandler;

        [Header("Configurations")]
        [SerializeField] private List<SettingsActionConfig> sceneConfigs;
        [SerializeField] private SettingsActionConfig defaultConfig;

        private void Awake()
        {
            Debug.Log("[Settings] Hệ thống Setting đã khởi chạy (Awake).");
            if (mainButton == null) mainButton = GetComponent<Button>();
            
            if (mainButton != null)
                mainButton.onClick.AddListener(OnMainButtonClicked);
            else
                Debug.LogError("[Settings] Không tìm thấy Component Button trên GameObject này!");
            
            if (menuUI != null)
                menuUI.OnActionClicked += HandleActionClicked;
        }

        private void OnDestroy()
        {
            if (menuUI != null)
                menuUI.OnActionClicked -= HandleActionClicked;
        }

        private void OnMainButtonClicked()
        {
            if (menuUI == null) return;

            // NẾU MENU ĐANG MỞ -> ĐÓNG LẠI (TOGGLE)
            if (menuUI.IsVisible)
            {
                Debug.Log("[Settings] Menu đang mở, thực hiện đóng lại.");
                menuUI.Hide();
                return;
            }

            Debug.Log("[Settings] Đã bấm nút chính. Đang tìm Config...");

            // Tìm config phù hợp với scene hiện tại
            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log($"[Settings] Scene hiện tại: '{currentScene}'");
            
            SettingsActionConfig activeConfig = null;

            foreach (var config in sceneConfigs)
            {
                if (config != null && config.sceneName == currentScene)
                {
                    activeConfig = config;
                    Debug.Log($"[Settings] Tìm thấy Config khớp với Scene: {config.name}");
                    break;
                }
            }

            if (activeConfig == null) 
            {
                activeConfig = defaultConfig;
                if (activeConfig != null) Debug.Log("[Settings] Dùng Default Config.");
            }

            if (activeConfig != null)
            {
                Debug.Log($"[Settings] Đang hiển thị Menu với {activeConfig.actions.Count} nút.");
                menuUI.Setup(activeConfig.actions, mainButton.transform.position);
            }
            else
            {
                Debug.LogError("[Settings] THẤT BẠI: Không tìm thấy Config nào (cả Scene lẫn Default)!");
            }
        }

        private void HandleActionClicked(SettingsActionData action)
        {
            if (actionHandler != null)
            {
                actionHandler.ExecuteAction(action);
            }
        }
    }
}
