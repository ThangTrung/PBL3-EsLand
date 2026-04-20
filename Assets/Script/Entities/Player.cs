using System;
using Script.Inventory.Controller;
using Script.Inventory.UI;
using UnityEngine;

namespace Script.Entities
{
    public class Player : Character
    {
        [Header("Farming Settings")]
        [SerializeField] private float farmRange = 1.5f;
        [SerializeField] private LayerMask resourceLayer; 
        [SerializeField] private int attackDamage = 1;    

        [Header("Survival Stats")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float maxThirst = 100f;
        [SerializeField] private float maxStamina = 100f;
        
        [Header("Inventory")]
        [SerializeField] protected GameObject inventory;

        private InventoryUI _ui;
        private InventoryController _inventoryController;
        private float Hunger { get; set; }
        private float Thirst { get; set; }
        public float Stamina { get; private set; }

        protected override void Awake()
        {
            
            base.Awake();
            Hunger = maxHunger;
            Thirst = maxThirst;
            Stamina = maxStamina;
        }

        private void Start()
        {  
            var instance = Instantiate(inventory, transform.position, transform.rotation);
            _inventoryController = instance.GetComponentInChildren<InventoryController>();
            _ui = _inventoryController.GetComponentInChildren<InventoryUI>();
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
            HandleMovementInput();
            HandleInventoryInput();
            HandleActionInput();
        }

        private void HandleActionInput()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
            {
                Attack();
            }
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
            // 1. Kiểm tra Cooldown và Thể lực
            if (!CanAttack())
                return;

            // 4. Dò tìm mỏ quặng trong phạm vi farmRange
            Collider2D[] hitResources = Physics2D.OverlapCircleAll(transform.position, farmRange, resourceLayer);

            if (hitResources.Length > 0)
            {
                var node = hitResources[0].GetComponent<ResourceNode>();
                if (node != null)
                {
                    // Lấy "giá" thể lực từ chính mỏ quặng
                    float currentCost = node.GetStaminaCost();

                    if (Stamina >= currentCost)
                    {
                        animator.SetTrigger("attack");
                        node.GetHit(attackDamage);

                        // Trừ theo giá trị động
                        Stamina -= currentCost;
                        Debug.Log($"[FARM] Đã trừ {currentCost} thể lực.");
                    }
                    else
                    {
                        Debug.LogWarning("Mệt quá, không đủ sức!");
                    }
                }
            }

            AttackTimer = baseAttackCooldown;
        }
    }
}