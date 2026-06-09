using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using EsLand.Core.Contracts.Audio;
using EsLand.Data.Audio;

namespace EsLand.Infrastructure.Audio
{
    /// <summary>
    /// Triển khai thực tế của IAudioService.
    /// Quản lý việc phát âm thanh thông qua Object Pooling và AudioMixer.
    /// </summary>
    public class AudioManager : MonoBehaviour, IAudioService
    {
        public static AudioManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int _initialPoolSize = 10;
        [SerializeField] private AudioMixer _mainMixer;

        [Header("BGM Channels")]
        [SerializeField] private AudioSource _bgmSource1;
        [SerializeField] private AudioSource _bgmSource2;

        private Stack<AudioSource> _sfxPool = new Stack<AudioSource>();
        private List<AudioSource> _activeSfx = new List<AudioSource>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePool()
        {
            for (int i = 0; i < _initialPoolSize; i++)
            {
                _sfxPool.Push(CreateNewAudioSource());
            }
        }

        private AudioSource CreateNewAudioSource()
        {
            GameObject sfxObj = new GameObject("SFX_Source");
            sfxObj.transform.SetParent(transform);
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        public void PlaySFX(AudioData data, Vector3 position = default)
        {
            if (data == null || data.Clip == null) return;

            AudioSource source = _sfxPool.Count > 0 ? _sfxPool.Pop() : CreateNewAudioSource();
            
            // Thiết lập vị trí nếu là 3D sound
            if (position != default)
            {
                source.transform.position = position;
                source.spatialBlend = 1f;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = 1f;
                source.maxDistance = 20f;
            }
            else
            {
                source.spatialBlend = 0f;
            }

            source.clip = data.Clip;
            source.outputAudioMixerGroup = data.MixerGroup;
            source.volume = data.Volume;
            source.pitch = data.GetRandomizedPitch();
            source.loop = data.Loop;

            source.Play();
            _activeSfx.Add(source);

            // Tự động trả về pool sau khi phát xong (nếu không loop)
            if (!data.Loop)
            {
                StartCoroutine(ReturnToPoolAfterFinished(source, data.Clip.length));
            }
        }

        private System.Collections.IEnumerator ReturnToPoolAfterFinished(AudioSource source, float duration)
        {
            yield return new WaitForSeconds(duration);
            source.Stop();
            _activeSfx.Remove(source);
            _sfxPool.Push(source);
        }

        public void PlayBGM(AudioData data, bool fade = true)
        {
            if (data == null || data.Clip == null) return;

            // Logic fade giữa 2 nguồn BGM có thể triển khai ở đây
            _bgmSource1.clip = data.Clip;
            _bgmSource1.outputAudioMixerGroup = data.MixerGroup;
            _bgmSource1.volume = data.Volume;
            _bgmSource1.loop = true;
            _bgmSource1.Play();
        }

        public void StopBGM(float fadeTime = 1.0f)
        {
            _bgmSource1.Stop();
            _bgmSource2.Stop();
        }

        public void SetVolume(string parameterName, float volume)
        {
            if (_mainMixer == null) return;
            
            // Chuyển đổi giá trị tuyến tính 0-1 sang dB cho Mixer
            float dB = (volume <= 0) ? -80f : Mathf.Log10(volume) * 20f;
            _mainMixer.SetFloat(parameterName, dB);
        }
    }
}
