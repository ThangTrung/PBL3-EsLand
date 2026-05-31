using System.Collections;
using Core.Contracts.Combat;
using UnityEngine;

namespace Gameplay.Combat.Feedback
{
    public class CombatFeedbackController : MonoBehaviour
    {
        [Header("Flash Settings")]
        [SerializeField] private Material flashMaterial; 
        [SerializeField] private float flashDuration = 0.2f;
        
        [Header("Knockback Settings")]
        [SerializeField] private float knockbackForce = 12f; 
        [SerializeField] private float knockbackDuration = 0.25f;

        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rb;
        private IDamageable _health;
        private Material _originalMaterial;
        private Coroutine _flashRoutine;
        private Coroutine _knockbackRoutine;
        private bool _isKnockedBack;

        private void Awake() { InitializeReferences(); }

        private void InitializeReferences()
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_health == null) _health = GetComponent<IDamageable>();
            
            if (_spriteRenderer != null && _originalMaterial == null)
            {
                _originalMaterial = _spriteRenderer.material;
            }
        }

        private void OnEnable()
        {
            InitializeReferences();
            if (_health != null) _health.OnDamageTaken += HandleDamage;
        }

        private void OnDisable()
        {
            if (_health != null) _health.OnDamageTaken -= HandleDamage;
            _isKnockedBack = false;
            if (_spriteRenderer != null && _originalMaterial != null) _spriteRenderer.material = _originalMaterial;
            
            if (_flashRoutine != null) { StopCoroutine(_flashRoutine); _flashRoutine = null; }
            if (_knockbackRoutine != null) { StopCoroutine(_knockbackRoutine); _knockbackRoutine = null; }
        }

        private void HandleDamage(float damage, Gameplay.Characters.Character source)
        {
            if (damage <= 0) return;

            if (_spriteRenderer != null)
            {
                if (_flashRoutine != null) StopCoroutine(_flashRoutine);
                _flashRoutine = StartCoroutine(FlashRoutine());
            }

            if (_rb != null)
            {
                Vector3 sourcePos = source != null ? source.transform.position : transform.position - (Vector3)Random.insideUnitCircle;
                ApplyKnockback(sourcePos);
            }
        }

        private IEnumerator FlashRoutine()
        {
            if (flashMaterial != null) _spriteRenderer.material = flashMaterial;
            yield return new WaitForSeconds(flashDuration);
            if (_originalMaterial != null) _spriteRenderer.material = _originalMaterial;
            _flashRoutine = null;
        }

        private void ApplyKnockback(Vector3 sourcePosition)
        {
            // If already being knocked back, we restart it (allows force stacking feel)
            if (_knockbackRoutine != null) StopCoroutine(_knockbackRoutine);
            
            Vector2 dir = (transform.position - sourcePosition).normalized;
            if (dir == Vector2.zero) dir = Random.insideUnitCircle.normalized;
            
            _knockbackRoutine = StartCoroutine(KnockbackRoutine(dir));
        }

        private IEnumerator KnockbackRoutine(Vector2 direction)
        {
            _isKnockedBack = true;
            var enemyMove = GetComponent<Gameplay.AI.Movement.EnemyMovementController>();
            if (enemyMove != null) enemyMove.SetCanMove(false);

            _rb.velocity = Vector2.zero;
            _rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

            yield return new WaitForSeconds(knockbackDuration);

            // [ROBUSTNESS FIX] Actively slow down the object after knockback ends
            // This prevents objects with 0 drag from flying forever.
            float brakeTime = 0.1f;
            float elapsed = 0f;
            Vector2 startVel = _rb.velocity;
            
            while (elapsed < brakeTime)
            {
                elapsed += Time.deltaTime;
                if (_rb != null) _rb.velocity = Vector2.Lerp(startVel, Vector2.zero, elapsed / brakeTime);
                yield return null;
            }

            if (_rb != null) _rb.velocity = Vector2.zero;

            if (enemyMove != null) enemyMove.SetCanMove(true);
            _isKnockedBack = false;
            _knockbackRoutine = null;
        }
    }
}