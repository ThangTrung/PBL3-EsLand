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
        }

        public void SetEquipmentState(bool isOpen)
        {
            IsEquipmentOpenInternal = isOpen;
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
