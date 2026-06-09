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
        
        // [Phase 1: Anti-Spam] Biến lưu trữ Coroutine tương tác hiện tại
        private Coroutine _currentInteractionRoutine;
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

            // Nếu đang trong thời gian cooldown, từ chối lệnh click mới luôn (Chống Spam xếp hàng)
            if (_attackTimer > 0) return;

            // Hủy trình tự tương tác cũ nếu có click mới
            if (_currentInteractionRoutine != null)
            {
                StopCoroutine(_currentInteractionRoutine);
            }

            _movement.SetFollowTarget(targetTransform, interactionRange, () => 
            {
                FaceTarget(targetTransform.position);
                // Lưu lại Coroutine đang chạy
                _currentInteractionRoutine = StartCoroutine(ExecuteAttackSequence(target, targetTransform));
            });
        }

        private System.Collections.IEnumerator ExecuteAttackSequence(IInteractable specificTarget, Transform targetTransform = null)
        {
            if (specificTarget == null) 
            {
                _currentInteractionRoutine = null;
                yield break;
            }

            if (!CanAttack()) 
            {
                _currentInteractionRoutine = null;
                yield break;
            }
            
            if (!specificTarget.CanInteract(_facade))
            {
                _currentInteractionRoutine = null;
                yield break; 
            }

            // Determine stamina cost dynamically via the interface
            float staminaCost = specificTarget.GetStaminaCost(_facade);

            // Try to consume stamina
            if (_survival != null && staminaCost > 0f)
            {
                if (!_survival.TryConsumeStamina(staminaCost))
                {
                    _currentInteractionRoutine = null;
                    yield break;
                }
            }

            _attackTimer = baseAttackCooldown;
            TriggerInteractAnimation();

            yield return new WaitForSeconds(interactionDelay);

            // [Phase 2] Xác thực khoảng cách (Impact Validation) triệt để - FIXED
            bool isTargetValid = true;

            // Kiểm tra xem quái có bị destroy/despawn trong lúc chờ Animation không
            if (targetTransform == null || specificTarget as UnityEngine.Object == null)
            {
                isTargetValid = false;
            }
            else
            {
                float validDistance = interactionRange + 0.3f;
                var targetCollider = targetTransform.GetComponent<Collider2D>();
                var myCollider = _facade != null ? _facade.GetComponent<Collider2D>() : null;

                if (targetCollider != null && myCollider != null)
                {
                    // Đo khoảng cách giữa 2 viền vật lý (Boundary-to-Boundary)
                    if (!myCollider.IsTouching(targetCollider))
                    {
                        var result = Physics2D.Distance(myCollider, targetCollider);
                        if (result.distance > validDistance)
                        {
                            isTargetValid = false;
                        }
                    }
                }
                else
                {
                    // Fallback cho trường hợp không có Collider: Cho phép tâm-đến-tâm lới lỏng (NavMesh Tolerance)
                    float centerDist = Vector2.Distance(transform.position, targetTransform.position);
                    if (centerDist > Mathf.Max(validDistance, 1.8f))
                    {
                        isTargetValid = false;
                    }
                }
            }

            if (isTargetValid)
            {
                specificTarget.Interact(_facade);
            }

            // Dọn dẹp khi kết thúc
            _currentInteractionRoutine = null;
        }

        // [Phase 3] Hook ngắt tương tác dành cho Movement/Input Controller
        public void CancelInteraction()
        {
            if (_currentInteractionRoutine != null)
            {
                StopCoroutine(_currentInteractionRoutine);
                _currentInteractionRoutine = null;
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
