using UnityEngine;
using UnityEngine.Audio;

namespace EsLand.Data.Audio
{
    /// <summary>
    /// ScriptableObject chứa cấu hình cho một âm thanh duy nhất.
    /// Cho phép thiết lập volume, pitch và mixer group một cách linh hoạt.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAudioData", menuName = "EsLand/Audio/AudioData")]
    public class AudioData : ScriptableObject
    {
        [Header("Audio Clip")]
        [SerializeField] private AudioClip _clip;
        
        [Header("Settings")]
        [SerializeField] private AudioMixerGroup _mixerGroup;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField, Range(0.1f, 3f)] private float _pitch = 1f;
        
        [Header("Randomization")]
        [SerializeField, Range(0f, 0.5f)] private float _pitchRandomRange = 0f;
        
        [Header("Looping")]
        [SerializeField] private bool _loop = false;

        // Getters
        public AudioClip Clip => _clip;
        public AudioMixerGroup MixerGroup => _mixerGroup;
        public float Volume => _volume;
        public bool Loop => _loop;

        /// <summary>
        /// Trả về pitch có kèm theo sự ngẫu nhiên nếu được thiết lập.
        /// </summary>
        public float GetRandomizedPitch()
        {
            if (_pitchRandomRange <= 0) return _pitch;
            return _pitch + Random.Range(-_pitchRandomRange, _pitchRandomRange);
        }
    }
}
