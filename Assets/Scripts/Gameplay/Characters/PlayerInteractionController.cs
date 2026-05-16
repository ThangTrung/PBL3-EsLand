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
            _animator = GetComponentInChildren<Animator>();
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
            if (!CanAttack()) return;

            // Manual attack without target (swinging in air)
            _attackTimer = baseAttackCooldown;
            if (_animator != null)
            {
                _animator.SetTrigger(InteractHash);
            }
            
            // We can still try to find a target in front if needed, 
            // but the priority is now on explicit targeted interaction.
            var target = FindInteractableTarget();
            target?.Interact(Facade);
        }
        public void InteractWithTarget(IInteractable target, Transform targetTransform)
        {
            if (target == null || targetTransform == null) return;

            var movement = GetComponent<PlayerMovementController>();
            if (movement != null)
            {
                // We stop only when touching the box collider of the target
                movement.SetFollowTarget(targetTransform, 0f, () => 
                {
                    // Rotate to face target
                    Vector3 dir = (targetTransform.position - transform.position).normalized;
                    if (dir.x != 0) transform.localScale = new Vector3(Mathf.Sign(dir.x), 1, 1);
                    
                    // Attack
                    if (CanAttack())
                    {
                        _attackTimer = baseAttackCooldown;
                        if (_animator != null) _animator.SetTrigger(InteractHash);
                        
                        // Apply damage specifically to this target
                        target.Interact(Facade);
                        Debug.Log($"Target {targetTransform.name} hit by targeted interaction.");
                    }
                });
            }
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
            // Center the interaction circle slightly above the feet (e.g., at waist level)
            Vector3 interactionOrigin = transform.position + new Vector3(0, 0.5f, 0);
            var hitCount = Physics2D.OverlapCircleNonAlloc(interactionOrigin, interactionRange, _hitResults, interactableLayer);
            
            if (hitCount <= 0) return null;

            for (var i = 0; i < hitCount; i++)
            {
                if (!_hitResults[i]) continue;
                if (_hitResults[i].TryGetComponent<IInteractable>(out var interactable))
                {
                    return interactable;
                }
            }

            return null;
        }
    }
}
