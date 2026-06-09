using UnityEngine;

namespace Infrastructure.Networking
{
    [CreateAssetMenu(fileName = "NetworkSettings", menuName = "Settings/Network Settings")]
    public class NetworkSettings : ScriptableObject
    {
        [Header("Backend Configuration")]
        [Tooltip("Địa chỉ cơ sở của server (ví dụ: http://localhost:3000 hoặc http://123.456.7.8:3000)")]
        [SerializeField] private string baseUrl = "http://localhost:3000";

        public string BaseUrl => baseUrl;

        public string LoginUrl => $"{baseUrl}/api/auth/login";
        public string SaveGameUrl => $"{baseUrl}/api/savegame";
        public string LoadGameUrl => $"{baseUrl}/api/loadgame/";

        // Singleton-like access for runtime convenience
        private static NetworkSettings _instance;
        public static NetworkSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<NetworkSettings>("NetworkSettings");
                }
                return _instance;
            }
        }
    }
}
