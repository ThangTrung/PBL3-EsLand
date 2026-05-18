using System;
using UnityEngine;
using Core.Events;

namespace Gameplay.Characters
{
    /// <summary>
    /// Specific implementation of Character for the Player.
    /// Handles player-specific logic like input coordination and UI state.
    /// </summary>
    [RequireComponent(typeof(PlayerMovementController))]
    [RequireComponent(typeof(PlayerInputController))]
    [RequireComponent(typeof(PlayerSurvivalController))]
    public class Player : Character
    {
        public event Action OnToggleInventory;
        public event Action OnToggleEquipment;
        public event Action<bool> OnUIStateChanged;

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
            
            // Prevent movement when UI is open
            if (IsAnyUIOpen && _movement != null)
            {
                _movement.StopMovement();
            }
        }
    }
}
