using UnityEngine;
using Gameplay.World;
using EsLand.Data.Audio;
using EsLand.Infrastructure.Audio;

namespace EsLand.Gameplay.Audio
{
    /// <summary>
    /// Lắng nghe sự kiện từ ResourceNode (Cây, Đá) để phát âm thanh khi bị khai thác.
    /// </summary>
    public class ResourceAudioListener : BaseAudioListener
    {
        [Header("Target")]
        [SerializeField] private ResourceNode _resourceNode;

        [Header("Audio Data")]
        [SerializeField] private AudioData _hitSound;

        protected override void SubscribeEvents()
        {
            if (_resourceNode == null) _resourceNode = GetComponent<ResourceNode>();
            
            if (_resourceNode != null)
            {
                _resourceNode.OnDamaged += HandleHit;
            }
        }

        protected override void UnsubscribeEvents()
        {
            if (_resourceNode != null)
            {
                _resourceNode.OnDamaged -= HandleHit;
            }
        }

        private void HandleHit()
        {
            PlaySound(_hitSound, transform.position);
        }
    }
}
