using Core.Contracts.Combat;
using Gameplay.AI;
using Gameplay.Combat.StatusEffects;
using Gameplay.Characters;
using Infrastructure.Pooling;
using UnityEngine;
using Core.Contracts.Shared;

namespace Gameplay.Combat.Projectiles
{
    public class HexProjectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float maxLifetime = 5f;
        [SerializeField] private float hexDuration = 5f;

        private Transform _owner;
        private Vector3 _direction;
        private float _lifeTimer;
        private bool _active;

        public void Initialize(Transform owner, Transform target)
        {
            _owner = owner;
            _direction = target != null ? (target.position - transform.position).normalized : transform.right;
            _lifeTimer = 0f;
            _active = true;
        }

        public void OnSpawn() => _active = true;
        public void OnReturn() => _active = false;

        private void Update()
        {
            if (!_active) return;

            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= maxLifetime)
            {
                ObjectPoolManager.Instance.Return(gameObject);
                return;
            }

            transform.position += _direction * speed * Time.deltaTime;

            // Simple collision check
            var hit = Physics2D.OverlapCircle(transform.position, 0.2f);
            if (hit != null && hit.transform != _owner)
            {
                HandleHit(hit.gameObject);
                ObjectPoolManager.Instance.Return(gameObject);
            }
        }

        private void HandleHit(GameObject target)
        {
            // 1. Check for IHexable (Enemies)
            if (target.TryGetComponent<IHexable>(out var hexable))
            {
                hexable.OnHexed();
                return;
            }

            // 2. Check for Player (apply effect)
            if (target.CompareTag("Player") && target.TryGetComponent<StatusEffectController>(out var status))
            {
                status.ApplyEffect(new PigTransformEffect(hexDuration));
            }
        }
    }
}