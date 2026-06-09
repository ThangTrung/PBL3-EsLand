using UnityEngine.SceneManagement;

namespace Core.SceneManagement
{
    /// <summary>
    /// Static class cung cấp API đơn giản để chuyển cảnh qua màn hình Loading.
    /// Cách sử dụng: SceneLoader.Load("TênSceneĐích");
    /// </summary>
    public static class SceneLoader
    {
        // Tên scene Loading cố định
        private const string LOADING_SCENE_NAME = "Loading";
        
        // Lưu trữ tên scene đích để scene Loading đọc được
        public static string TargetSceneName { get; private set; }

        public static void Load(string sceneName)
        {
            TargetSceneName = sceneName;
            
            // Chuyển hướng ngay lập tức sang scene Loading
            SceneManager.LoadScene(LOADING_SCENE_NAME);
        }
    }
}
