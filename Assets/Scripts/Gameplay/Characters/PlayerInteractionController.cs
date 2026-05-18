using Core.Contracts.Equipment;
using Core.Contracts.Shared;
using UnityEngine;

namespace Gameplay.Characters
{
    /// <summary>
    /// Handles player interactions with the world and combat targets.
    /// Manages interaction timing, animations, and damage calculation.
    /// </summary>
    public class PlayerInteractionController : MonoBehaviour
    {
        private static readonly int InteractHash = Animator.StringToHash("interact");

        [Header("Interaction Settings")]
        [SerializeField] private float interactionDelay = 0.4f; // Delay for animation swing to hit
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float baseAttackCooldown = 1f;

        private float _attackTimer;
        private Character _facade;
        private PlayerMovementController _movement;

        private void Awake()
        {
            _facade = GetComponentInParent<Character>();
            _movement = GetComponentInParent<PlayerMovementController>();
        }

        private void Update()
        {
            if (_attackTimer > 0)
                _attackTimer -= Time.deltaTime;
        }

        /// <summary>
        /// Handles a direct interaction attempt via click.
        /// </summary>
        public void HandleInteractionClick(Vector2 mouseWorldPos)
        {
            // Use RaycastAll to hit multiple objects at the point, ensuring we don't get blocked
            // by non-interactable colliders (like the player's own body) that might be overlapping.
            RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero, 0f, interactableLayer);
            
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.TryGetComponent<IInteractable>(out var target))
                {
                    Debug.Log($"[Interaction] Targeting {hit.collider.name} for interaction.");
                    InteractWithTarget(target, hit.collider.transform);
                    return; // Only interact with the first valid target found
                }
            }
        }

        /// <summary>
        /// Initiates an interaction with a specific target, moving to it first if needed.
        /// </summary>
        public void InteractWithTarget(IInteractable target, Transform targetTransform)
        {
            if (target == null || targetTransform == null || _movement == null) return;

            // Auto-move to target and perform action on arrival
            _movement.SetFollowTarget(targetTransform, 0f, () => 
            {
                FaceTarget(targetTransform.position);
                StartCoroutine(ExecuteAttackSequence(target));
            });
        }

        private System.Collections.IEnumerator ExecuteAttackSequence(IInteractable specificTarget)
        {
            if (specificTarget == null) yield break;

            // Wait for cooldown if necessary
            while (_attackTimer > 0) yield return null;

            if (!CanAttack()) yield break;

            _attackTimer = baseAttackCooldown;
            TriggerInteractAnimation();

            // Wait for the "hit" frame in animation
            yield return new WaitForSeconds(interactionDelay);

            // Re-verify target is still valid before executing
            if (specificTarget != null)
            {
                specificTarget.Interact(_facade);
            }
        }

        private void FaceTarget(Vector3 targetPos)
        {
            float lookDir = targetPos.x > transform.position.x ? 1 : -1;
            // Always flip the root facade to ensure consistent visuals
            if (_facade != null)
                _facade.transform.localScale = new Vector3(lookDir, 1, 1);
            else
                transform.localScale = new Vector3(lookDir, 1, 1);
        }

        private void TriggerInteractAnimation()
        {
            if (_facade != null && _facade.Animator != null)
                _facade.Animator.SetTrigger(InteractHash);
        }

        private bool CanAttack()
        {
            return (_facade != null && (_facade.Health == null || !_facade.Health.IsDead)) && _attackTimer <= 0;
        }

        public float GetTotalDamage()
        {
            float total = baseDamage;
            if (_facade != null && _facade.EquipmentManager != null)
                total += _facade.EquipmentManager.GetTotalDamageModifier();
            return total;
        }
    }
}
