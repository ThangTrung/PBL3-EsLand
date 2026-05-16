using UnityEngine;
using Core.Contracts.Shared;


namespace Gameplay.Characters
{
    public class PlayerInputController : MonoBehaviour
    {
        private PlayerMovementController _movement;
        private PlayerInteractionController _interaction;
        private Player _playerFacade;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovementController>();
            _interaction = GetComponent<PlayerInteractionController>();
            _playerFacade = GetComponent<Player>();
        }

        private void Update()
        {
            if (_playerFacade == null) return;
            HandleUIInput();
            
            // Allow movement input even if follow target is active (it will cancel follow)
            HandleMovementInput();
            
            if (_playerFacade.IsAnyUIOpen)
            {
                _movement?.Move(Vector3.zero);
                return;
            }
            
            HandleActionInput();
        }

        private void HandleMovementInput()
        {
            if (_movement == null) return;

            var moveX = Input.GetAxisRaw("Horizontal");
            var moveY = Input.GetAxisRaw("Vertical");
            var inputDirection = new Vector3(moveX, moveY, 0f);
            
            // Only call Move if we are actually pressing keys 
            // OR if we are NOT auto-moving (to allow stopping)
            if (inputDirection.sqrMagnitude > 0.01f || !_movement.IsFollowingTarget)
            {
                _movement.Move(inputDirection);
            }
        }

        private void HandleActionInput()
        {
            if (_interaction == null) return;

            if (Input.GetMouseButtonDown(0))
            {
                // Delegate world position conversion and interaction logic to the specialist controller
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                _interaction.HandleInteractionClick(mousePos);
            }
        }

        private void HandleUIInput()
        {
            if (_playerFacade == null) return;

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _playerFacade.ToggleInventory();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                _playerFacade.ToggleEquipment();
            }
        }
    }
}
