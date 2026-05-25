using Core.Contracts.Combat;
using Core.Contracts.Shared;
using Data.Combat;
using Gameplay.Characters;
using Gameplay.Combat.StatusEffects;
using Infrastructure.Pooling;
using UnityEngine;
using System.Collections;

namespace Gameplay.Combat.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
    public class BombProjectile : MonoBehaviour, IProjectile, IPoolable
    {
        private ProjectileSpec _spec;
        private Transform _owner;
        private float _lifeTimer;
        private bool _initialized;
        private bool _hasExploded;
        private bool _hasLanded;

        private Rigidbody2D _rb;
        private Animator _animator;
        private static readonly Collider2D[] _hitBuffer = new Collider2D[15];

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
        }

        public void Initialize(ProjectileSpec spec, Transform owner, Transform target)
        {
            _spec = spec;
            _owner = owner;
            _lifeTimer = 0f;
            _initialized = true;
            _hasExploded = false;
            _hasLanded = false;

            _rb.isKinematic = false;
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;

            if (target != null)
            {
                // TÍNH TOÁN QUỸ ĐẠO PARABOL (VẬT LÝ THỰC TẾ)
                Vector2 startPos = transform.position;
                Vector2 targetPos = target.position;
                
                // Quy định thời gian bay trên không (càng nhỏ bay càng nhanh)
                float flightTime = 1.0f; // Bạn có thể tinh chỉnh số này
                
                // Vận tốc ngang = Quãng đường ngang / thời gian
                float velocityX = (targetPos.x - startPos.x) / flightTime;
                
                // Vận tốc dọc = (Quãng đường dọc - 0.5 * gia tốc * thời gian bình phương) / thời gian
                float gravity = Physics2D.gravity.y * _rb.gravityScale;
                float velocityY = (targetPos.y - startPos.y - 0.5f * gravity * (flightTime * flightTime)) / flightTime;

                // Áp dụng lực bắn ngay lập tức (Thay đổi vận tốc trực tiếp)
                _rb.velocity = new Vector2(velocityX, velocityY);
                
                // Xoáy bom tùy theo hướng ném
                float spinDirection = (targetPos.x > startPos.x) ? -1f : 1f;
                _rb.AddTorque(spinDirection * 50f);
            }

            // Gọi Animation
            _animator.Play("Spin");
        }

        public void OnSpawn()
        {
            _lifeTimer = 0f;
            _initialized = false;
            _hasExploded = false;
            _hasLanded = false;
            
            if (_rb != null)
            {
                _rb.velocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }
        }

        public void OnReturn()
        {
            _initialized = false;
        }

        private void Update()
        {
            if (!_initialized || _spec == null || _hasExploded) return;

            _lifeTimer += Time.deltaTime;
            
            // Nổ khi hết thời gian chờ (Fuse)
            if (_lifeTimer >= _spec.MaxLifetime)
            {
                Explode();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_hasExploded || _hasLanded) return;

            // Nếu bom va chạm với bất kỳ thứ gì (chạm đất/tường), bắt đầu chuyển sang animation cháy (Burn)
            // (Độ nảy sẽ do PhysicsMaterial2D tự động xử lý)
            _hasLanded = true;
            _animator.Play("Burn");
        }

        private void Explode()
        {
            _hasExploded = true;
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.isKinematic = true; // Dừng mọi lực vật lý, đứng im tại chỗ khi nổ
            
            _animator.Play("Explode");

            // 1. Sát thương AOE
            int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, _spec.HitRadius, _hitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                var hit = _hitBuffer[i];
                if (hit == null) continue;
                if (_owner != null && hit.transform == _owner) continue;

                if (hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    Character source = _owner != null ? _owner.GetComponent<Character>() : null;
                    damageable.TakeDamage(_spec.BaseDamage, source);
                }

                if (hit.TryGetComponent<StatusEffectController>(out var statusController))
                {
                    if (_spec.ApplyPoison)
                        statusController.ApplyEffect(new PoisonEffect(_spec.PoisonDps, _spec.PoisonDuration, _owner != null ? _owner.GetComponent<Character>() : null));
                    if (_spec.ApplySlow)
                        statusController.ApplyEffect(new SlowEffect(_spec.SlowMultiplier, _spec.SlowDuration));
                }
            }

            // 2. Thu hồi về Pool sau khi hiệu ứng nổ kết thúc
            StartCoroutine(WaitAnimationAndReturn());
        }

        private IEnumerator WaitAnimationAndReturn()
        {
            // Chờ 1 frame để Animator kịp cập nhật state
            yield return null; 
            
            float animLength = 0.5f; // Mặc định nếu không tìm thấy
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Explode")) 
            {
                animLength = stateInfo.length;
            }

            // Đợi animation nổ chạy xong
            yield return new WaitForSeconds(animLength);
            
            ObjectPoolManager.Instance.Return(gameObject);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (_spec != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, _spec.HitRadius);
            }
        }
    }
}
