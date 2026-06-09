using Core.Contracts.Equipment;
using Core.Contracts.Shared;
using Gameplay.Components;
using UI.Cursor;
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
        private Highlight _currentHover;
        private Camera _cachedCamera;

        private Camera MainCamera
        {
            get
            {
                if (_cachedCamera == null) _cachedCamera = Camera.main;
                return _cachedCamera;
            }
        }

        private void Awake()
        {
            InitializeReferences();
        }

        private void InitializeReferences()
        {
            if (!_facade) _facade = GetComponentInParent<Character>();
            if (!_movement) _movement = GetComponentInParent<PlayerMovementController>();
            if (!_survival) _survival = GetComponentInParent<PlayerSurvivalController>();
        }

        private void Update()
        {
            if (_attackTimer > 0)
                _attackTimer -= Time.deltaTime;

            // Nếu đang trong chế độ xây dựng, không thực hiện các tương tác khác (Highlight, Click)
            if (Gameplay.Building.BuildingPlacementManager.Instance != null && 
                Gameplay.Building.BuildingPlacementManager.Instance.IsPlacing)
            {
                if (_currentHover != null)
                {
                    _currentHover.SetHighlight(false);
                    _currentHover = null;
                }
                return;
            }

            HandleHover();
        }

        private void HandleHover()
        {
            if (MainCamera == null) return;

            var screenPos = Input.mousePosition;
            screenPos.z = Mathf.Abs(MainCamera.transform.position.z);
            Vector2 mouseWorldPos = MainCamera.ScreenToWorldPoint(screenPos);

            Collider2D[] colliders = Physics2D.OverlapCircleAll(mouseWorldPos, 0.2f, interactableLayer);
            Highlight newHover = null;

            foreach (var col in colliders)
            {
                if (col == null) continue;
                
                // Tìm Highlight ở chính nó hoặc object cha (Hỗ trợ cấu hình InteractionZone là con)
                newHover = col.GetComponent<Highlight>() ?? col.GetComponentInParent<Highlight>();
                if (newHover != null) break;
            }

            if (newHover != _currentHover)
            {
                if (_currentHover != null) _currentHover.SetHighlight(false);
                _currentHover = newHover;
                if (_currentHover != null)
                {
                    _currentHover.SetHighlight(true);
                    UpdateInteractionCursor(newHover);
                }
                else
                {
                    if (CursorManager.Instance != null) CursorManager.Instance.SetNormalCursor();
                }
            }
        }

        private void UpdateInteractionCursor(Highlight hover)
        {
            if (CursorManager.Instance == null) return;

            var interactable = hover.GetComponent<IInteractable>() ?? hover.GetComponentInParent<IInteractable>();
            if (interactable == null)
            {
                CursorManager.Instance.SetNormalCursor();
                return;
            }

            if (interactable.CanInteract(_facade))
            {
                CursorManager.Instance.SetPointerCursor();
            }
            else
            {
                CursorManager.Instance.SetForbiddenCursor();
            }
        }

        public void HandleInteractionClick(Vector3 mouseWorldPos)
        {
            // Ngăn tương tác click nếu đang xây dựng
            if (Gameplay.Building.BuildingPlacementManager.Instance != null && 
                Gameplay.Building.BuildingPlacementManager.Instance.IsPlacing) return;

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
            if (specificTarget == null) 
            {
                yield break;
            }

            while (_attackTimer > 0) yield return null;

            if (!CanAttack()) 
            {
                yield break;
            }
            
            if (!specificTarget.CanInteract(_facade))
            {
                yield break; 
            }

            // Determine stamina cost dynamically via the interface
            float staminaCost = specificTarget.GetStaminaCost(_facade);

            // Try to consume stamina
            if (_survival != null && staminaCost > 0f)
            {
                if (!_survival.TryConsumeStamina(staminaCost))
                {
                    yield break;
                }
            }

            _attackTimer = baseAttackCooldown;
            TriggerInteractAnimation();

            yield return new WaitForSeconds(interactionDelay);

            if (specificTarget != null)
            {
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

            if (_survival != null)
            {
                total *= _survival.GetDamageMultiplier();
            }
            return total;
        }
    }
}
