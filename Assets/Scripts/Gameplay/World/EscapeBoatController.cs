using UnityEngine;
using Core.Contracts.Shared;
using Core;
using Gameplay.Characters;

namespace Gameplay.World
{
    /// <summary>
    /// Script gắn vào Thuyền để kết thúc game khi người chơi tương tác.
    /// </summary>
    public class EscapeBoatController : MonoBehaviour, IInteractable
    {
        public string InteractionAnimationTrigger => "interact";

        public bool CanInteract(Character interactor)
        {
            // Chỉ cho phép tương tác nếu đây là Thuyền thật (đã được người chơi chế tạo và đặt xuống)
            return true;
        }

        public float GetStaminaCost(Character interactor)
        {
            return 0f; // Không tốn thể lực để thoát đảo
        }

        public void Interact(Character interactor)
        {
            Debug.Log("<color=green>[Escape]</color> Người chơi đã lên thuyền thoát đảo!");
            
            // Gọi logic chiến thắng từ GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.HandleVictory();
            }
            else
            {
                Debug.LogError("[Escape] Không tìm thấy GameManager Instance!");
            }
        }
    }
}
