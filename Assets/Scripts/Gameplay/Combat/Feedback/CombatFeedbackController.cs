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
        }

        private void HandleDamage(float damage, Gameplay.Characters.Character source)
        {
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
            if (flashMaterial != null)
            {
                _spriteRenderer.material = flashMaterial;
            }
            yield return new WaitForSeconds(flashDuration);
            
            if (_originalMaterial != null)
            {
                _spriteRenderer.material = _originalMaterial;
            }
            _flashRoutine = null;
        }

        private void ApplyKnockback(Vector3 sourcePosition)
        {
            if (_isKnockedBack || _rb == null) return;
            Vector2 dir = (transform.position - sourcePosition).normalized;
            if (dir == Vector2.zero) dir = Random.insideUnitCircle.normalized;
            StartCoroutine(KnockbackRoutine(dir));
        }

        private IEnumerator KnockbackRoutine(Vector2 direction)
        {
            _isKnockedBack = true;
            var enemyMove = GetComponent<Gameplay.AI.Movement.EnemyMovementController>();
            var playerMove = GetComponent<Gameplay.Characters.PlayerMovementController>();

            if (enemyMove != null) enemyMove.SetCanMove(false);
            if (playerMove != null) playerMove.SetCanMove(false);

            _rb.velocity = Vector2.zero;
            _rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

            yield return new WaitForSeconds(knockbackDuration);

            if (enemyMove != null) enemyMove.SetCanMove(true);
            if (playerMove != null) playerMove.SetCanMove(true);
            _isKnockedBack = false;
        }
    }
}