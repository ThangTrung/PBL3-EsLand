using Core.Contracts.Equipment;
using UnityEngine;

namespace Gameplay.Characters
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovementController : MonoBehaviour
    {
        private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

        [Header("Settings")]
        [SerializeField] private float baseMoveSpeed = 5f;

        private Rigidbody2D _rb;
        private Animator _animator;
        private IEquipmentController _equipmentController;
        private CharacterHealth _health;

        private bool _canMove = true;
        private float _speedMultiplier = 1f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _equipmentController = GetComponent<IEquipmentController>();
            _health = GetComponent<CharacterHealth>();
        }

        private void Start()
        {
            if (_health != null)
            {
                _health.OnDie += HandleDie;
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnDie -= HandleDie;
            }
        }

        private void HandleDie()
        {
            StopMovement();
            _canMove = false;
        }

        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
            if (!canMove)
            {
                StopMovement();
            }
        }

        public void Move(Vector3 direction)
        {
            if (!_canMove)
            {
                StopMovement();
                return;
            }

            var movement = direction.normalized * GetMoveSpeed();
            _rb.velocity = new Vector2(movement.x, movement.y);

            var isMoving = direction.sqrMagnitude > 0;
            if (_animator != null)
            {
                _animator.SetBool(IsMovingHash, isMoving);
            }

            if (direction.x != 0)
            {
                transform.localScale = new Vector3(Mathf.Sign(direction.x), 1, 1);
            }
        }

        public void StopMovement()
        {
            if (_rb != null) _rb.velocity = Vector2.zero;
            if (_animator != null) _animator.SetBool(IsMovingHash, false);
        }

        public void SetBaseMoveSpeed(float speed)
        {
            baseMoveSpeed = speed;
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Max(0.1f, multiplier);
        }

        public float GetCurrentMoveSpeed() => GetMoveSpeed();

        private float GetMoveSpeed()
        {
            var speed = baseMoveSpeed;
            if (_equipmentController != null)
                speed += _equipmentController.GetTotalSpeedModifier();
            speed *= _speedMultiplier;
            return Mathf.Max(1f, speed);
        }
    }
}
