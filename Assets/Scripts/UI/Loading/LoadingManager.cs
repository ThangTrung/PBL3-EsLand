using System.Collections;
using Core.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Loading
{
    /// <summary>
    /// Quản lý logic load scene bất đồng bộ và đảm bảo thời gian load tối thiểu.
    /// </summary>
    public class LoadingManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float minLoadingTime = 2.5f;
        
        [Header("UI References")]
        [SerializeField] private Slider progressBar;

        private void Start()
        {
            // Bắt đầu quá trình load
            string target = SceneLoader.TargetSceneName;
            
            // Nếu TargetSceneName trống (ví dụ mở scene Loading trực tiếp), quay về Menu hoặc in lỗi
            if (string.IsNullOrEmpty(target))
            {
                Debug.LogWarning("[LoadingManager] TargetSceneName is empty! Defaulting to 'MainMenu'.");
                target = "MainMenu";
            }

            StartCoroutine(LoadAsync(target));
        }

        private IEnumerator LoadAsync(string sceneName)
        {
            Debug.Log($"[LoadingManager] Starting async load for scene: {sceneName}");
            yield return null; // Đợi 1 frame để UI khởi tạo xong

            float startTime = Time.time;
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            
            // Không cho phép tự động kích hoạt scene ngay khi load xong 
            // để chúng ta kiểm soát thời gian tối thiểu.
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                // Progress của Unity chạy từ 0 -> 0.9. Khi đạt 0.9 là đã load xong data.
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                
                if (progressBar != null)
                {
                    progressBar.value = progress;
                }

                // Nếu đã load xong data (0.9) và đã qua thời gian tối thiểu
                if (operation.progress >= 0.9f)
                {
                    Debug.Log($"[LoadingManager] Scene {sceneName} progress: {operation.progress}. Waiting for min time...");
                    if ((Time.time - startTime) >= minLoadingTime)
                    {
                        Debug.Log($"[LoadingManager] Activation allowed for {sceneName}");
                        operation.allowSceneActivation = true;
                    }
                }

                yield return null;
            }
            Debug.Log($"[LoadingManager] Scene {sceneName} load COMPLETED.");
        }
    }
}
