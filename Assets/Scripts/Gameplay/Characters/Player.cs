using System;
using Data.Equipment;
using UnityEngine;
using Core.Events;

namespace Gameplay.Characters
{
    [RequireComponent(typeof(PlayerMovementController))]
    [RequireComponent(typeof(PlayerInputController))]
    [RequireComponent(typeof(PlayerInteractionController))]
    [RequireComponent(typeof(PlayerEquipmentAnimator))]
    public class Player : Character
    {
        public event Action OnToggleInventory;
        public event Action OnToggleEquipment;
        public event Action<bool> OnUIStateChanged;

        [Header("Debug Test")]
        public Data.Equipment.Equipment TestPickaxe;

        [Header("Survival Stats")]
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float maxThirst = 100f;
        [SerializeField] private float maxStamina = 100f;

        public bool IsInventoryOpenInternal { get; private set; }
        public bool IsEquipmentOpenInternal { get; private set; }

                public bool IsAnyUIOpen => IsInventoryOpenInternal || IsEquipmentOpenInternal;

        [Header("Runtime Stats")]
        [SerializeField] private float currentHunger;
        [SerializeField] private float currentThirst;
        [SerializeField] private float currentStamina;

        public float MaxHunger => maxHunger;
        public float MaxThirst => maxThirst;
        public float MaxStamina => maxStamina;

        public float CurrentHunger => currentHunger;
        public float CurrentThirst => currentThirst;
        public float CurrentStamina => currentStamina;

        public event Action<float> OnHungerChanged;
        public event Action<float> OnThirstChanged;
        public event Action<float> OnStaminaChanged;

        protected override void Awake()
        {
            base.Awake();
            currentHunger = maxHunger;
            currentThirst = maxThirst;
            currentStamina = maxStamina;
        }

        private void Update()
        {
            // Increased drain for clear testing: 5 units per second
            ConsumeHunger(5.0f * Time.deltaTime);
            ConsumeThirst(7.0f * Time.deltaTime);
        }

        public void ConsumeHunger(float amount)
        {
            currentHunger = Mathf.Clamp(currentHunger - amount, 0, maxHunger);
            OnHungerChanged?.Invoke(currentHunger);
        }

        public void ConsumeThirst(float amount)
        {
            currentThirst = Mathf.Clamp(currentThirst - amount, 0, maxThirst);
            OnThirstChanged?.Invoke(currentThirst);
        }

        public void AddHunger(float amount)
        {
            currentHunger = Mathf.Clamp(currentHunger + amount, 0, maxHunger);
            OnHungerChanged?.Invoke(currentHunger);
        }

        public void AddThirst(float amount)
        {
            currentThirst = Mathf.Clamp(currentThirst + amount, 0, maxThirst);
            OnThirstChanged?.Invoke(currentThirst);
        }

        private void Start()
        {
            GameEvents.OnPlayerReady?.Invoke(this);
        }

        public void ToggleInventory()
        {
            Debug.Log("Toggling Inventory UI");
            OnToggleInventory?.Invoke();
        }

        public void ToggleEquipment()
        {
            Debug.Log("Toggling Equipment UI");
            OnToggleEquipment?.Invoke();
        }

        public void SetInventoryState(bool isOpen)
        {
            IsInventoryOpenInternal = isOpen;
            UpdateUIState();
        }

        public void SetEquipmentState(bool isOpen)
        {
            IsEquipmentOpenInternal = isOpen;
            UpdateUIState();
        }

        private void UpdateUIState()
        {
            OnUIStateChanged?.Invoke(IsAnyUIOpen);
            
            if (IsAnyUIOpen)
            {
                var movement = GetComponent<PlayerMovementController>();
                if (movement != null)
                {
                    movement.StopMovement();
                }
            }
        }

        public void EquipTestItem()
        {
            if (EquipmentManager != null && TestPickaxe != null)
            {
                EquipmentManager.Equip(TestPickaxe);
            }
        }

        public void Unequip(Data.Equipment.EquipSlot slot)
        {
            if (EquipmentManager != null)
                EquipmentManager.Unequip(slot);
        }
    }
}
