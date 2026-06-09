using UnityEngine;
using Gameplay.Characters;
using EsLand.Data.Audio;

namespace EsLand.Gameplay.Audio
{
    /// <summary>
    /// Lắng nghe các sự kiện từ CharacterHealth (Player hoặc Enemy) để phát âm thanh tương ứng.
    /// </summary>
    public class HealthAudioListener : BaseAudioListener
    {
        [Header("Target")]
        [SerializeField] private CharacterHealth _health;

        [Header("Audio Data")]
        [SerializeField] private AudioData _hitSound;
        [SerializeField] private AudioData _deathSound;

        protected override void SubscribeEvents()
        {
            if (_health == null) _health = GetComponent<CharacterHealth>();
            
            if (_health != null)
            {
                _health.OnDamaged += HandleHit;
                _health.OnDie += HandleDeath;
            }
        }

        protected override void UnsubscribeEvents()
        {
            if (_health != null)
            {
                _health.OnDamaged -= HandleHit;
                _health.OnDie -= HandleDeath;
            }
        }

        private void HandleHit()
        {
            PlaySound(_hitSound, transform.position);
        }

        private void HandleDeath()
        {
            PlaySound(_deathSound, transform.position);
        }
    }
}
