using System;
using Data.Equipment;
using UnityEngine;
using Core.Events;

namespace Gameplay.Characters
{
    /// <summary>
    /// High-level facade for the Player character.
    /// Handles UI state coordination and global player events.
    /// </summary>
    [RequireComponent(typeof(PlayerMovementController))]
    [RequireComponent(typeof(PlayerInputController))]
    [RequireComponent(typeof(PlayerInteractionController))]
    [RequireComponent(typeof(PlayerEquipmentAnimator))]
    [RequireComponent(typeof(PlayerSurvivalController))]
    public class Player : Character
    {
        public event Action OnToggleInventory;
        public event Action OnToggleEquipment;
        public event Action<bool> OnUIStateChanged;

        [Header("Debug Test")]
        public Data.Equipment.Equipment TestPickaxe;

        private bool _isInventoryOpen;
        private bool _isEquipmentOpen;
        
        public bool IsInventoryOpenInternal => _isInventoryOpen;
        public bool IsEquipmentOpenInternal => _isEquipmentOpen;
        public bool IsAnyUIOpen => _isInventoryOpen || _isEquipmentOpen;

        private PlayerMovementController _movement;

        protected override void Awake()
        {
            base.Awake();
            _movement = GetComponent<PlayerMovementController>();
        }

        private void Start()
        {
            GameEvents.OnPlayerReady?.Invoke(this);
        }

        public void ToggleInventory() => OnToggleInventory?.Invoke();
        public void ToggleEquipment() => OnToggleEquipment?.Invoke();

        public void SetInventoryState(bool isOpen) => SetUIState(isOpen, ref _isInventoryOpen);
        public void SetEquipmentState(bool isOpen) => SetUIState(isOpen, ref _isEquipmentOpen);

        private void SetUIState(bool newState, ref bool targetField)
        {
            targetField = newState;
            OnUIStateChanged?.Invoke(IsAnyUIOpen);
            
            if (IsAnyUIOpen && _movement != null)
            {
                _movement.StopMovement();
            }
        }

        [ContextMenu("Debug/Equip Test Pickaxe")]
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
