using System;
using Script.Equipment.Core;
using Script.Equipment.Interfaces;
using Script.Items;
using Script.Shared.Interfaces;
using UnityEngine;

namespace Script.Entities
{
    public class Player : Character
    {
        private static readonly int Interact = Animator.StringToHash("interact");

        public override Script.Inventory.Interfaces.IInventory Inventory => GetComponent<Script.Inventory.Interfaces.IInventory>();
        public override IEquipmentManager EquipmentManager => _equipmentManager;
        private IEquipmentManager _equipmentManager;

        public event Action OnToggleInventory;
        public event Action OnToggleEquipment;
        public event Action<bool> OnUIStateChanged;

        [Header("Debug Test")]
        public Script.Items.Equipment testPickaxe;

        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 1.5f;
        [SerializeField] private LayerMask interactableLayer;

        [Header("Survival Stats")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float maxThirst = 100f;
        [SerializeField] private float maxStamina = 100f;

        private bool IsAnyUIOpen => IsInventoryOpenInternal || IsEquipmentOpenInternal;

        protected override void Awake()
        {
            base.Awake();
            _equipmentManager = GetComponent<IEquipmentManager>();
            if (_equipmentManager == null)
                return;
            _equipmentManager.Initialize(this);
            _equipmentManager.OnItemEquipped += HandleItemEquipped;
            _equipmentManager.OnItemUnequipped += HandleItemUnequipped;
        }

        private void HandleItemEquipped(EquipSlot slot, IEquippable item)
        {
            if (slot != EquipSlot.MainHand || item is not Items.Equipment equipment || !animator)
                return;
            if (equipment.overrideController)
                animator.runtimeAnimatorController = equipment.overrideController;
            else
                ResetAnimator();
        }

        private void HandleItemUnequipped(EquipSlot slot, IEquippable item)
        {
            if (slot == EquipSlot.MainHand && animator)
                ResetAnimator();
        }

        private void Start() { }

        protected override void Update()
        {
            base.Update();

            if (IsAnyUIOpen)
            {
                StopMovement();
                HandleUIInput();
                return;
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                if (testPickaxe) Equip(testPickaxe);
            }
            HandleMovementInput();
            HandleUIInput();
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

        private void HandleUIInput()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                ToggleInventory();

            if (Input.GetKeyDown(KeyCode.E))
                ToggleEquipment();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void ToggleInventory()
        {
            Debug.Log("Toggling Inventory UI");
            OnToggleInventory?.Invoke();
        }

        private void ToggleEquipment()
        {
            Debug.Log("Toggling Equipment UI");
            OnToggleEquipment?.Invoke();
        }

        public bool IsInventoryOpenInternal { get; private set; }
        public bool IsEquipmentOpenInternal { get; private set; }

        public void SetUIState(bool inventoryOpen, bool equipmentOpen)
        {
            IsInventoryOpenInternal = inventoryOpen;
            IsEquipmentOpenInternal = equipmentOpen;

            OnUIStateChanged?.Invoke(IsAnyUIOpen);

            if (IsAnyUIOpen)
                StopMovement();
        }

        private void StopMovement()
        {
            rb.velocity = Vector2.zero;
            animator.SetBool(Animator.StringToHash("isMoving"), false);
        }

        public override void Attack()
        {
            if (!CanAttack())
                return;

            AttackTimer = baseAttackCooldown;
            animator.SetTrigger(Interact);

            var target = FindInteractableTarget();
            target?.Interact(this);
        }

        #region Stats & Equipment
        public override float GetTotalDamage()
        {
            var total = baseDamage;
            if (EquipmentManager != null)
                total += EquipmentManager.GetTotalDamageModifier();
            return total;
        }

        protected override float GetMaxHealth()
        {
            var total = maxHealth;
            if (EquipmentManager != null)
                total += EquipmentManager.GetTotalHealthModifier();
            return total;
        }

        protected override float GetTotalDefense()
        {
            var total = baseDefense;
            if (EquipmentManager != null)
                total += EquipmentManager.GetTotalDefenseModifier();
            return total;
        }

        protected override float GetMoveSpeed()
        {
            var speed = baseMoveSpeed;
            if (EquipmentManager != null)
                speed += EquipmentManager.GetTotalSpeedModifier();
            return Mathf.Max(1f, speed);
        }

        private void Equip(IEquippable item)
        {
            if (EquipmentManager != null)
                EquipmentManager.Equip(item);
        }

        public void Unequip(EquipSlot slot)
        {
            if (EquipmentManager != null)
                EquipmentManager.Unequip(slot);
        }
        #endregion

        private readonly Collider2D[] _hitResults = new Collider2D[10];
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
