using UnityEngine;

namespace Core.Diagnostics
{
    /// <summary>
    /// ProjectGuard - Một công cụ bảo vệ toàn cục giúp phát hiện các cấu hình Scene sai sót 
    /// (như Camera Size = 0) ngay lập tức trong Editor và Runtime.
    /// </summary>
    [ExecuteInEditMode]
    public class ProjectGuard : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool checkOnStart = true;
        [SerializeField] private bool continuousCheckInEditor = true;

        private void Start()
        {
            if (checkOnStart) PerformCheck();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying && continuousCheckInEditor)
            {
                PerformCheck();
            }
        }
#endif

        public void PerformCheck()
        {
            CheckCameras();
            CheckAudio();
            CheckEventSystem();
        }

        private void CheckCameras()
        {
            Camera[] cameras = FindObjectsOfType<Camera>();
            if (cameras.Length == 0)
            {
                Debug.LogError("[ProjectGuard] KHÔNG tìm thấy Camera nào trong Scene!");
                return;
            }

            foreach (var cam in cameras)
            {
                if (cam.orthographic)
                {
                    if (cam.orthographicSize <= 0.001f)
                    {
                        Debug.LogError($"[ProjectGuard] Camera '{cam.name}' có Orthographic Size = {cam.orthographicSize}. UI và Sprite sẽ không hiển thị!", cam);
                    }
                }
                else
                {
                    if (cam.fieldOfView <= 0.001f)
                    {
                        Debug.LogError($"[ProjectGuard] Camera '{cam.name}' có FOV = {cam.fieldOfView}. Hình ảnh sẽ bị lỗi!", cam);
                    }
                }
            }
        }

        private void CheckAudio()
        {
            if (FindObjectOfType<AudioListener>() == null)
            {
                Debug.LogWarning("[ProjectGuard] Scene thiếu AudioListener. Bạn sẽ không nghe thấy tiếng game.");
            }
        }

        private void CheckEventSystem()
        {
            if (FindObjectOfType<UnityEngine.Canvas>() != null && FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                Debug.LogError("[ProjectGuard] Có Canvas nhưng THIẾU EventSystem. Các nút bấm (Button) sẽ không hoạt động!");
            }
        }
    }
}
