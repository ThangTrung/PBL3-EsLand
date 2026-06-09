using UnityEngine;
using EsLand.Data.Audio;
using EsLand.Infrastructure.Audio;

namespace EsLand.Gameplay.Audio
{
    /// <summary>
    /// Script đơn giản để kích hoạt âm thanh nhặt đồ.
    /// Có thể gọi thông qua Animation Event hoặc Unity Event khi vật phẩm bị tiêu hủy.
    /// </summary>
    public class PickupAudioTrigger : MonoBehaviour
    {
        [SerializeField] private AudioData _pickupSound;

        public void PlayPickupSound()
        {
            if (AudioManager.Instance != null && _pickupSound != null)
            {
                AudioManager.Instance.PlaySFX(_pickupSound, transform.position);
            }
        }
        
        // Tự động gọi khi Object bị Disable (ví dụ khi biến mất để vào túi đồ)
        private void OnDisable()
        {
            // Chỉ phát nếu object bị tắt do logic nhặt đồ, tránh phát khi tắt game
            if (gameObject.activeInHierarchy == false && Time.time > 0.1f)
            {
                PlayPickupSound();
            }
        }
    }
}
