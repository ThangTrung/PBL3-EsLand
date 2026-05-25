using Core.Contracts.Combat;
using Data.Combat;
using Gameplay.Combat.StatusEffects;
using Gameplay.Characters;
using UnityEngine;
using Core.Contracts.Shared;
using Infrastructure.Pooling;

namespace Gameplay.Combat.Projectiles
{
    public class Projectile2D : MonoBehaviour, IProjectile, IPoolable
    {
        private ProjectileSpec _spec;
        private Transform _owner;
        private Transform _target;
        private Vector3 _direction;
        private float _lifeTimer;
        private bool _initialized;
        
        private static readonly Collider2D[] _hitBuffer = new Collider2D[10];
private SpriteRenderer _spriteRenderer;
        private Animator _animator;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
        }

        public void Initialize(ProjectileSpec spec, Transform owner, Transform target)
        {
            _spec = spec;
            _owner = owner;
            _target = target;
            _lifeTimer = 0f;
            _initialized = true;

            // Nếu Prefab có Animator (như dùng ảnh sprite sheet để xoay), ta KHÔNG ghi đè ảnh Sprite tĩnh
            if (_animator == null)
            {
                if (_spec != null && _spec.ProjectileSprite != null)
                {
                    if (_spriteRenderer == null)
                    {
                        _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                    }
                    _spriteRenderer.sprite = _spec.ProjectileSprite;
                }
            }

            _direction = _target != null ? (_target.position - transform.position).normalized : transform.right;

            // Tự động xoay hướng mũi đạn (dành cho các đạn cần chĩa mũi về phía trước như Harpoon)
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            float rotationOffset = _spec != null ? _spec.SpriteRotationOffset : 0f;
            transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
            
            // Gọi Animator chạy state "Spin" nếu có
            if (_animator != null)
            {
                _animator.Play("Spin", -1, 0f);
            }
        }

public void OnSpawn()
        {
            _lifeTimer = 0f;
            _initialized = false;
        }

        public void OnReturn()
        {
            _initialized = false;
        }


        private void Update()
        {
            if (!_initialized || _spec == null)
            {
                ObjectPoolManager.Instance.Return(gameObject);
                return;
            }

            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= _spec.MaxLifetime)
            {
                ObjectPoolManager.Instance.Return(gameObject);
                return;
            }

            // Bay theo hướng đã định
            transform.position += _direction * _spec.Speed * Time.deltaTime;

            // Xoay tròn liên tục nếu có cài đặt tốc độ xoay (SpinSpeed > 0 hoặc < 0)
            if (_spec.SpinSpeed != 0f)
            {
                transform.Rotate(0f, 0f, _spec.SpinSpeed * Time.deltaTime);
            }

            int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, _spec.HitRadius, _hitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                var hit = _hitBuffer[i];
                if (hit == null) continue;
                if (hit.isTrigger) continue; // Bỏ qua các trigger vô hình (như vùng nhìn thấy)
                if (_owner != null && hit.transform.root == _owner.root) continue; // Bỏ qua chính bản thân người ném

                if (hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    Character source = null;
                    if (_owner != null)
                    {
                        source = _owner.GetComponent<Character>();
                    }

                    damageable.TakeDamage(_spec.BaseDamage, source);
                }

                if (hit.TryGetComponent<StatusEffectController>(out var statusController))
                {
                    if (_spec.ApplyPoison)
                    {
                        var poison = new PoisonEffect(_spec.PoisonDps, _spec.PoisonDuration, _owner != null ? _owner.GetComponent<Character>() : null);
                        statusController.ApplyEffect(poison);
                    }

                    if (_spec.ApplySlow)
                    {
                        var slow = new SlowEffect(_spec.SlowMultiplier, _spec.SlowDuration);
                        statusController.ApplyEffect(slow);
                    }
                }

                // Nếu không có tính năng xuyên thấu, đụng mục tiêu rắn là biến mất
                if (!_spec.CanPierce)
                {
                    ObjectPoolManager.Instance.Return(gameObject);
                    return;
                }
            }
        }
    }
}
