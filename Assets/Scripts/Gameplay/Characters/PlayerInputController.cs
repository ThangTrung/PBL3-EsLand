using UnityEngine;
using Core.Contracts.Shared;


namespace Gameplay.Characters
{
    public class PlayerInputController : MonoBehaviour
    {
        private PlayerMovementController _movement;
        private PlayerInteractionController _interaction;
        private Player _playerFacade;
        private Camera _mainCamera;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovementController>();
            _interaction = GetComponentInChildren<PlayerInteractionController>();
            _playerFacade = GetComponent<Player>();
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null) _mainCamera = GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            if (_playerFacade == null) return;
            
            HandleUIInput();
            HandleMovementInput();
            
            if (!_playerFacade.IsAnyUIOpen)
            {
                HandleActionInput();
            }
            else
            {
                _movement?.Move(Vector3.zero);
            }
        }

        private void HandleMovementInput()
        {
            if (_movement == null) return;

            var input = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0f);
            
            if (input.sqrMagnitude > 0.01f || !_movement.IsFollowingTarget)
            {
                _movement.Move(input);
            }
        }

        private void HandleActionInput()
        {
            if (_interaction == null || _mainCamera == null) return;

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                _interaction.HandleInteractionClick(mousePos);
            }
        }

        private void HandleUIInput()
        {
            if (Input.GetKeyDown(KeyCode.Tab)) _playerFacade.ToggleInventory();
            if (Input.GetKeyDown(KeyCode.E)) _playerFacade.ToggleEquipment();
        }
    }
}
