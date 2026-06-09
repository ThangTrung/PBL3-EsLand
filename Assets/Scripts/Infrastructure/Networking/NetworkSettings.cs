using UnityEngine;

namespace Infrastructure.Networking
{
    [CreateAssetMenu(fileName = "NetworkSettings", menuName = "EsLand/Config/NetworkSettings")]
    public class NetworkSettings : ScriptableObject
    {
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

        [Header("Server Configuration")]
        [SerializeField] private string serverIp = "localhost";
        [SerializeField] private int port = 3000;

        public string ServerIp => serverIp;
        public string LoginUrl => $"http://{serverIp}:{port}/api/auth/login";
        public string RegisterUrl => $"http://{serverIp}:{port}/api/auth/register";
    }
}
