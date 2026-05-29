using Core.Contracts.Combat;
using Core.Contracts.Shared;
using Data.Combat;
using Gameplay.AI.Animation;
using Gameplay.Characters;
using Gameplay.Combat.StatusEffects;
using Infrastructure.Pooling;
using UnityEngine;

namespace Gameplay.Combat.Projectiles
{
    public class BombProjectile : MonoBehaviour, IProjectile, IPoolable
    {
        [Header("Explosion Settings")]
        [SerializeField] private float explosionRadius = 2.5f;
        [SerializeField] private GameObject explosionEffectPrefab;

        private ProjectileSpec _spec;
        private Transform _owner;
        private Vector3 _startPosition;
        private Vector3 _targetPosition;
        private float _flightDuration;
        private float _elapsedTime;
        private bool _initialized;
        private float _arcHeight = 2.0f;

        private SpriteRenderer _spriteRenderer;
        private static readonly Collider2D[] _hitBuffer = new Collider2D[20];

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(ProjectileSpec spec, Transform owner, Transform target)
        {
            if (target == null)
            {
                ObjectPoolManager.Instance.Return(gameObject);
                return;
            }

            _spec = spec;
            _owner = owner;
            _startPosition = transform.position;
            _targetPosition = target.position;
            
            float distance = Vector2.Distance(_startPosition, _targetPosition);
            _flightDuration = distance / (spec != null ? spec.Speed : 5f);
            if (_flightDuration < 0.2f) _flightDuration = 0.5f;

            _elapsedTime = 0f;
            _initialized = true;
            _arcHeight = Mathf.Clamp(distance * 0.5f, 1.5f, 4f);

            if (_spriteRenderer != null && spec != null && spec.ProjectileSprite != null)
            {
                _spriteRenderer.sprite = spec.ProjectileSprite;
            }
        }

        public void OnSpawn()
        {
            _elapsedTime = 0f;
            _initialized = false;
        }

        public void OnReturn()
        {
            _initialized = false;
        }

        private void Update()
        {
            if (!_initialized) return;

            _elapsedTime += Time.deltaTime;
            float t = _elapsedTime / _flightDuration;

            if (t >= 1.0f)
            {
                Explode();
                return;
            }

            Vector3 currentPos = Vector3.Lerp(_startPosition, _targetPosition, t);
            float heightOffset = 4 * _arcHeight * t * (1 - t);
            currentPos.y += heightOffset;

            transform.position = currentPos;

            if (_spec != null && _spec.SpinSpeed != 0f)
            {
                transform.Rotate(0f, 0f, _spec.SpinSpeed * Time.deltaTime);
            }
        }

        private void Explode()
        {
            if (explosionEffectPrefab != null)
            {
                ObjectPoolManager.Instance.Get(explosionEffectPrefab, _targetPosition, Quaternion.identity);
            }

            int hitCount = Physics2D.OverlapCircleNonAlloc(_targetPosition, explosionRadius, _hitBuffer);
            
            for (int i = 0; i < hitCount; i++)
            {
                var hit = _hitBuffer[i];
                if (hit == null || hit.isTrigger) continue;
                if (_owner != null && hit.transform.root == _owner.root) continue;

                if (hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    Character source = _owner != null ? _owner.GetComponent<Character>() : null;
                    damageable.TakeDamage(_spec != null ? _spec.BaseDamage : 10f, source);
                }

                if (hit.TryGetComponent<StatusEffectController>(out var statusController) && _spec != null)
                {
                    if (_spec.ApplyPoison)
                    {
                        statusController.ApplyEffect(new PoisonEffect(_spec.PoisonDps, _spec.PoisonDuration, _owner?.GetComponent<Character>()));
                    }
                    if (_spec.ApplySlow)
                    {
                        statusController.ApplyEffect(new SlowEffect(_spec.SlowMultiplier, _spec.SlowDuration));
                    }
                }
            }

            _initialized = false;
            ObjectPoolManager.Instance.Return(gameObject);
        }
    }
}