using UnityEngine;
using EsLand.Infrastructure.Audio;
using EsLand.Data.Audio;

namespace EsLand.Gameplay.Audio
{
    public class SceneBGMPlayer : MonoBehaviour
    {
        [SerializeField] private AudioData _bgmToPlay;

        private void Start()
        {
            // Gọi AudioManager để phát nhạc ngay khi Scene bắt đầu
            if (AudioManager.Instance != null && _bgmToPlay != null)
            {
                AudioManager.Instance.PlayBGM(_bgmToPlay);
            }
        }
    }
}
