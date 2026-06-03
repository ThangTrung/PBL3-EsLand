using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void HandleVictory()
        {
            // Dừng thời gian
            Time.timeScale = 0;
            
            // Tìm và hiển thị UI chiến thắng (Sẽ thực hiện ở Task sau)
            Debug.Log("<color=gold>[VICTORY]</color> Chúc mừng! Bạn đã chiến thắng trò chơi!");
            
            // Phát sự kiện chiến thắng nếu cần
            Core.Events.GameEvents.RaiseVictory();
        }
    }
}
