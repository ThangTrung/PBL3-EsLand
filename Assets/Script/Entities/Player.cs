using Script.Interfaces;
using Script.Inventory.Controller;
using Script.Inventory.UI;
using UnityEngine;

namespace Script.Entities
{
    public class Player : Character
    {
        private static readonly int Interact = Animator.StringToHash("interact");

        [Header("Debug Test")]
        public Script.Items.Equipment testPickaxe;
        
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 1.5f;
        [SerializeField] private LayerMask interactableLayer;

        [Header("Survival Stats")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float maxThirst = 100f;
        [SerializeField] private float maxStamina = 100f;
        
        [Header("Inventory")]
        [SerializeField] protected GameObject inventory;

        private InventoryUI _ui;
        private InventoryController _inventoryController;

        private void Start()
        {
            var instance = Instantiate(inventory, transform.position, transform.rotation, transform);
            _inventoryController = instance.GetComponentInChildren<InventoryController>();
            _inventoryController.SetOwner(this);
            _ui = _inventoryController.GetComponentInChildren<InventoryUI>();
            _ui.SetInventoryProvider(_inventoryController);
        }

        protected override void Update()
        {
            base.Update();
            if (_ui && _ui.IsVisible) 
            {
                rb.velocity = Vector2.zero;
                animator.SetBool(Animator.StringToHash("isMoving"), false);
                HandleInventoryInput();
                return; 
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("1");
                if (testPickaxe) Equip(testPickaxe);
            }
            HandleMovementInput();
            HandleInventoryInput();
            HandleActionInput();
        }

        private void HandleActionInput()
        {
            if (Input.GetMouseButtonDown(0))
                Attack();
        }

        private void HandleMovementInput()
        {
            var moveX = Input.GetAxisRaw("Horizontal");
            var moveY = Input.GetAxisRaw("Vertical");
            var inputDirection = new Vector3(moveX, moveY, 0f);
            Move(inputDirection);
        }
        
        private void HandleInventoryInput()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                ToggleInventory();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void ToggleInventory()
        {
            var isOpening = !_ui.IsVisible;
            _ui.SetVisible(isOpening);
            if (!isOpening) 
                return;
            rb.velocity = Vector2.zero;
            animator.SetBool(Animator.StringToHash("isMoving"), false);
        }
        
        

        public override void Attack()
        {
            if (!CanAttack()) 
                return;

            AttackTimer = baseAttackCooldown;
            animator.SetTrigger("interact");

            var target = FindInteractableTarget();
            target?.Interact(this);
        }
        
        private readonly Collider2D[] _hitResults = new Collider2D[10];
        private IInteractable FindInteractableTarget()
        {
            var hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, interactionRange, _hitResults, interactableLayer);
            if (hitCount <= 0)
                return null;

            for (var i = 0; i < hitCount; i++)
            {
                if (!_hitResults[i])
                    continue;

                if (_hitResults[i].TryGetComponent<IInteractable>(out var interactable))
                    return interactable;
            }

            return null;
        }
    }
}