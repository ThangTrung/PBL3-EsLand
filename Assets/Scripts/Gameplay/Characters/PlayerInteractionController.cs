using Core.Contracts.Equipment;
using Core.Contracts.Shared;
using UnityEngine;

namespace Gameplay.Characters
{
    public class PlayerInteractionController : MonoBehaviour
    {
                private static readonly int InteractHash = Animator.StringToHash("interact");

        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 1.5f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float baseAttackCooldown = 1f;

        private float _attackTimer;
        private Animator _animator;
        private CharacterHealth _health;
        private IEquipmentController _equipmentController;

        private readonly Collider2D[] _hitResults = new Collider2D[10];
        
        // We temporarily pass 'this.gameObject' or 'Player.cs' facade as the source to IDamageable.
        // For now, exposing a getter to allow passing the Character facade if needed.
        private Character Facade { get; set; }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _health = GetComponent<CharacterHealth>();
            _equipmentController = GetComponent<IEquipmentController>();
            Facade = GetComponent<Character>();
        }

        private void Update()
        {
            if (_attackTimer > 0)
                _attackTimer -= Time.deltaTime;
        }

        public void AttemptAttack()
        {
            if (!CanAttack())
                return;

            _attackTimer = baseAttackCooldown;
            if (_animator != null)
            {
                _animator.SetTrigger(InteractHash);
            }

            var target = FindInteractableTarget();
            // In the new architecture, we pass the Facade (Player.cs) as the interaction source.
            target?.Interact(Facade);
        }

        private bool CanAttack()
        {
            var isDead = _health != null && _health.IsDead;
            return !isDead && _attackTimer <= 0;
        }

        public float GetTotalDamage()
        {
            var total = baseDamage;
            if (_equipmentController != null)
                total += _equipmentController.GetTotalDamageModifier();
            return total;
        }

        private IInteractable FindInteractableTarget()
        {
            var hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, interactionRange, _hitResults, interactableLayer);
            if (hitCount <= 0)
                return null;

            for (var i = 0; i < hitCount; i++)
            {
                if (!_hitResults[i]) continue;
                if (_hitResults[i].TryGetComponent<IInteractable>(out var interactable))
                    return interactable;
            }

            return null;
        }
    }
}
