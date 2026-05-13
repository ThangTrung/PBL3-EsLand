using UnityEngine;

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
            if (_playerFacade != null && _playerFacade.IsAnyUIOpen)
            {
                HandleUIInput();
                return;
            }

            HandleMovementInput();
            HandleUIInput();
            HandleActionInput();
        }

        private void HandleMovementInput()
        {
            if (!_movement) return;

            var moveX = Input.GetAxisRaw("Horizontal");
            var moveY = Input.GetAxisRaw("Vertical");
            var inputDirection = new Vector3(moveX, moveY, 0f);
            
            _movement.Move(inputDirection);
        }

        private void HandleActionInput()
        {
            if (!_interaction) return;

            if (Input.GetMouseButtonDown(0))
            {
                _interaction.AttemptAttack();
            }
        }

        private void HandleUIInput()
        {
            if (!_playerFacade) 
                return;

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Debug.Log("Tab key pressed - toggling inventory");
                _playerFacade.ToggleInventory();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                _playerFacade.ToggleEquipment();
            }
        }
    }
}
