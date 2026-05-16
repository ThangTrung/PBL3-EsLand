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
        [SerializeField] private float interactionDelay = 0.4f; // Delay for animation swing to hit
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float baseAttackCooldown = 1f;

        private float _attackTimer;
        private Character _facade;
        private PlayerMovementController _movement;

        private void Awake()
        {
            _facade = GetComponent<Character>();
            _movement = GetComponent<PlayerMovementController>();
        }

        private void Update()
        {
            if (_attackTimer > 0)
                _attackTimer -= Time.deltaTime;
        }

        public void HandleInteractionClick(Vector2 mouseWorldPos)
        {
            // Direct targeted interaction via mouse click
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, interactableLayer);
            
            if (hit.collider != null && hit.collider.TryGetComponent<IInteractable>(out var target))
            {
                Debug.Log($"Targeting {hit.collider.name} for interaction.");
                InteractWithTarget(target, hit.collider.transform);
            }
            else
            {
                AttemptAttack();
            }
        }

        public void AttemptAttack()
        {
            if (!CanAttack()) return;
            StartCoroutine(ExecuteAttackSequence(null));
        }

        public void InteractWithTarget(IInteractable target, Transform targetTransform)
        {
            if (target == null || targetTransform == null || _movement == null) return;

            Debug.Log($"[Interaction] Starting move to {targetTransform.name}");
            // Auto-move to target and perform action on arrival
            _movement.SetFollowTarget(targetTransform, 0f, () => 
            {
                FaceTarget(targetTransform.position);
                StartCoroutine(ExecuteAttackSequence(target));
            });
        }

        private System.Collections.IEnumerator ExecuteAttackSequence(IInteractable specificTarget)
        {
            // Wait for cooldown if necessary
            if (_attackTimer > 0)
            {
                Debug.Log($"[Interaction] Waiting for cooldown: {_attackTimer:F2}s");
                while (_attackTimer > 0) yield return null;
            }

            if (!CanAttack()) 
            {
                Debug.LogWarning("[Interaction] Cannot attack even after waiting (Dead?)");
                yield break;
            }

            _attackTimer = baseAttackCooldown;
            TriggerInteractAnimation();

            Debug.Log($"[Interaction] Animation started. Waiting {interactionDelay}s for hit.");
            yield return new WaitForSeconds(interactionDelay);

            if (specificTarget != null)
            {
                Debug.Log($"[Interaction] Executing Interact on {specificTarget}");
                specificTarget.Interact(_facade);
            }
            else
            {
                PerformPrecisionRaycast();
            }
        }

        private void PerformPrecisionRaycast()
        {
            Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            Vector2 origin = (Vector2)transform.position + new Vector2(0, 0.5f);
            
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, interactionRange, interactableLayer);
            
            if (hit.collider != null && hit.collider.TryGetComponent<IInteractable>(out var target))
            {
                target.Interact(_facade);
                Debug.Log($"Precision attack hit: {hit.collider.name}");
            }
        }

        private void FaceTarget(Vector3 targetPos)
        {
            float lookDir = targetPos.x > transform.position.x ? 1 : -1;
            transform.localScale = new Vector3(lookDir, 1, 1);
        }

        private void TriggerInteractAnimation()
        {
            if (_facade.Animator != null)
                _facade.Animator.SetTrigger(InteractHash);
        }

        private bool CanAttack()
        {
            return (_facade.Health == null || !_facade.Health.IsDead) && _attackTimer <= 0;
        }

        public float GetTotalDamage()
        {
            float total = baseDamage;
            if (_facade.EquipmentManager != null)
                total += _facade.EquipmentManager.GetTotalDamageModifier();
            return total;
        }
    }
}
