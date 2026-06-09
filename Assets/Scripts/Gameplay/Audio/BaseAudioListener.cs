using UnityEngine;
using EsLand.Infrastructure.Audio;
using EsLand.Data.Audio;

namespace EsLand.Gameplay.Audio
{
    /// <summary>
    /// Lớp cơ sở cho các thành phần lắng nghe sự kiện và phát âm thanh.
    /// Giúp tách biệt logic âm thanh khỏi logic gameplay chính.
    /// </summary>
    public abstract class BaseAudioListener : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            SubscribeEvents();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeEvents();
        }

        /// <summary>
        /// Đăng ký các sự kiện gameplay cần phát âm thanh.
        /// </summary>
        protected abstract void SubscribeEvents();

        /// <summary>
        /// Hủy đăng ký các sự kiện.
        /// </summary>
        protected abstract void UnsubscribeEvents();

        /// <summary>
        /// Phát âm thanh thông qua AudioManager.
        /// </summary>
        protected void PlaySound(AudioData data, Vector3 position = default)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(data, position);
            }
        }
    }
}
