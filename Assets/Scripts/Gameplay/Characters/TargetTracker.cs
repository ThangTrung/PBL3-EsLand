using Core.Contracts.Inventory;
using Core.Events;
using UnityEngine;

namespace Gameplay.Characters
{
    /// <summary>
    /// Lưu trữ tham chiếu tĩnh đến vị trí của Player.
    /// Giúp loại bỏ lệnh GameObject.FindGameObjectWithTag("Player") đắt đỏ trên toàn hệ thống AI.
    /// Bạn cần ĐẢM BẢO script này được gắn vào một GameManager hoặc GameObject tồn tại lâu dài trong Scene.
    /// </summary>
    public class TargetTracker : MonoBehaviour
    {
        public static Transform PlayerTarget { get; private set; }

        private void OnEnable()
        {
            GameEvents.OnPlayerReady += HandlePlayerReady;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerReady -= HandlePlayerReady;
        }

        private void HandlePlayerReady(IInventoryHolder playerInventory)
        {
            var component = playerInventory as Component;
            if (component != null)
            {
                PlayerTarget = component.transform;
            }
        }
    }
}
