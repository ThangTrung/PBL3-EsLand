using UnityEngine;
using EsLand.Data.Audio;

namespace EsLand.Core.Contracts.Audio
{
    /// <summary>
    /// Giao diện cho dịch vụ quản lý âm thanh.
    /// Cho phép các module gameplay phát âm thanh mà không cần biết chi tiết triển khai.
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Phát hiệu ứng âm thanh (SFX). Thường dùng cho các âm thanh ngắn.
        /// </summary>
        void PlaySFX(AudioData data, Vector3 position = default);

        /// <summary>
        /// Phát nhạc nền (BGM).
        /// </summary>
        /// <param name="data">Dữ liệu âm thanh.</param>
        /// <param name="fade">Có thực hiện hiệu ứng mờ dần hay không.</param>
        void PlayBGM(AudioData data, bool fade = true);

        /// <summary>
        /// Dừng nhạc nền đang phát.
        /// </summary>
        void StopBGM(float fadeTime = 1.0f);

        /// <summary>
        /// Điều chỉnh âm lượng thông qua tham số AudioMixer.
        /// </summary>
        void SetVolume(string parameterName, float volume);
    }
}
