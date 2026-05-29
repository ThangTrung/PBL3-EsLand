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
        [SerializeField] private float interactionDelay = 0.4f; 
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private float baseAttackCooldown = 1f;
        [SerializeField] private float interactionRange = 0.5f;

        private float _attackTimer;
        private Character _facade;
        private PlayerMovementController _movement;
        private PlayerSurvivalController _survival;
        private Camera _mainCamera;
        private Environment.Highlight _currentHover;

        private void Awake()
        {
            InitializeReferences();
        }

        private void Start()
        {
            InitializeReferences();
            _mainCamera = Camera.main;
            if (_mainCamera == null) _mainCamera = GetComponentInChildren<Camera>();
        }

        private void InitializeReferences()
        {
            if (!_facade) _facade = GetComponentInParent<Character>();
            if (!_movement) _movement = GetComponentInParent<PlayerMovementController>();
            if (!_survival) _survival = GetComponentInParent<PlayerSurvivalController>();
            
            // Fallback for detached prefabs
            if (!_facade && !transform.parent) _facade = transform.parent.GetComponent<Character>();
            if (!_movement && !transform.parent) _movement = transform.parent.GetComponent<PlayerMovementController>();
            if (!_survival && !transform.parent) _survival = transform.parent.GetComponent<PlayerSurvivalController>();
        }

        private void Update()
        {
            if (_attackTimer > 0)
                _attackTimer -= Time.deltaTime;

            HandleHover();
        }

        private void HandleHover()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            var screenPos = Input.mousePosition;
            screenPos.z = Mathf.Abs(_mainCamera.transform.position.z);
            Vector2 mouseWorldPos = _mainCamera.ScreenToWorldPoint(screenPos);

            Collider2D[] colliders = Physics2D.OverlapCircleAll(mouseWorldPos, 0.2f, interactableLayer);
            Environment.Highlight newHover = null;

            foreach (var col in colliders)
            {
                if (col == null) continue;
                
                // Tìm Highlight ở chính nó hoặc object cha (Hỗ trợ cấu hình InteractionZone là con)
                newHover = col.GetComponent<Gameplay.Environment.Highlight>() ?? col.GetComponentInParent<Gameplay.Environment.Highlight>();
                if (newHover != null) break;
            }

            if (newHover != _currentHover)
            {
                if (_currentHover != null) _currentHover.SetHighlight(false);
                _currentHover = newHover;
                if (_currentHover != null) _currentHover.SetHighlight(true);
            }
        }

        public void HandleInteractionClick(Vector3 mouseWorldPos)
        {
            var colliders = Physics2D.OverlapCircleAll(mouseWorldPos, 0.2f, interactableLayer);
            
            foreach (var col in colliders)
            {
                if (col == null) continue;

                // Tìm IInteractable ở chính nó hoặc cha
                var target = col.GetComponent<IInteractable>() ?? col.GetComponentInParent<IInteractable>();
                if (target == null) continue;

                InteractWithTarget(target, col.transform);
                return;
            }
        }

        private void InteractWithTarget(IInteractable target, Transform targetTransform)
        {
            InitializeReferences();
            if (target == null || !targetTransform  || !_movement) return;
            
            if (!target.CanInteract(_facade))
            {
                return;
            }

            _movement.SetFollowTarget(targetTransform, interactionRange, () => 
            {
                FaceTarget(targetTransform.position);
                StartCoroutine(ExecuteAttackSequence(target));
            });
        }

        private System.Collections.IEnumerator ExecuteAttackSequence(IInteractable specificTarget)
        {
            Debug.Log("[Combat] ExecuteAttackSequence started.");
            if (specificTarget == null) 
            {
                Debug.Log("[Combat] specificTarget is null, aborting.");
                yield break;
            }

            while (_attackTimer > 0) yield return null;

            if (!CanAttack()) 
            {
                Debug.Log($"[Combat] CanAttack() is false (Dead or timer). _attackTimer={_attackTimer}, Health={(_facade?.Health?.CurrentHealth)}");
                yield break;
            }
            
            if (!specificTarget.CanInteract(_facade))
            {
                Debug.Log("[Combat] specificTarget.CanInteract returned false.");
                yield break; 
            }

            // Determine stamina cost dynamically via the interface
            float staminaCost = specificTarget.GetStaminaCost(_facade);

            // Try to consume stamina
            if (_survival != null && staminaCost > 0f)
            {
                if (!_survival.TryConsumeStamina(staminaCost))
                {
                    Debug.Log($"[Combat] Not enough stamina. Cost: {staminaCost}, Current: {_survival.CurrentStamina}");
                    yield break;
                }
            }

            Debug.Log("[Combat] Triggering Attack Animation.");
            _attackTimer = baseAttackCooldown;
            TriggerInteractAnimation();

            yield return new WaitForSeconds(interactionDelay);

            if (specificTarget != null)
            {
                Debug.Log("[Combat] Delivering damage via specificTarget.Interact().");
                specificTarget.Interact(_facade);
            }
        }

        private void FaceTarget(Vector3 targetPos)
        {
            if (_facade == null) return;
            float lookDir = targetPos.x > transform.position.x ? 1 : -1;
            _facade.transform.localScale = new Vector3(lookDir, 1, 1);
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
