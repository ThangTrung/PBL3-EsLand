using System.Collections;
using Core.Contracts.Shared;
using UnityEngine;

namespace Gameplay.Characters
{
    /// <summary>
    /// Component quản lý logic chết và hồi sinh của Player.
    /// Tuân thủ nguyên lý SOLID và dễ dàng tích hợp với các hệ thống khác.
    /// </summary>
    [RequireComponent(typeof(CharacterHealth))]
    public class PlayerRespawnController : MonoBehaviour, IRespawnable
    {
        [Header("Animation Settings")]
        [SerializeField] private string deathStateName = "Death";       // Tên State trong Animator
        [SerializeField] private string deathTriggerName = "Die";       // Tên Trigger Parameter
        [SerializeField] private string respawnTriggerName = "Respawn"; // Tên Trigger Parameter
        
        [Header("Fallback Timers")]
        [SerializeField] private float defaultDeathDelay = 2.0f;        // Dùng nếu không tìm thấy clip

        public Vector3 RespawnPoint { get; set; }

        private CharacterHealth _health;
        private PlayerMovementController _movement;
        private PlayerInputController _input;
        private Animator _animator;
        private bool _isRespawning;

        // Hash IDs để tối ưu hiệu năng và tránh sai sót chuỗi
        private int _deathTriggerHash;
        private int _respawnTriggerHash;
        private int _deathStateHash;

        private void Awake()
        {
            _health = GetComponent<CharacterHealth>();
            _movement = GetComponent<PlayerMovementController>();
            _input = GetComponent<PlayerInputController>();
            _animator = GetComponentInChildren<Animator>() ?? GetComponent<Animator>();

            // Khởi tạo Hashes
            _deathTriggerHash = Animator.StringToHash(deathTriggerName);
            _respawnTriggerHash = Animator.StringToHash(respawnTriggerName);
            _deathStateHash = Animator.StringToHash(deathStateName);
        }

        private void Start()
        {
            if (RespawnPoint == Vector3.zero)
                RespawnPoint = transform.position;

            if (_health != null)
                _health.OnDie += HandlePlayerDie;
        }

        private void OnDestroy()
        {
            if (_health != null)
                _health.OnDie -= HandlePlayerDie;
        }

        public void SetRespawnPoint(Vector3 position)
        {
            RespawnPoint = position;
        }

        public void Respawn()
        {
            if (_isRespawning) return;
            StartCoroutine(RespawnRoutine());
        }

        private void HandlePlayerDie()
        {
            Respawn();
        }

        private IEnumerator RespawnRoutine()
        {
            _isRespawning = true;

            // 1. Khoá hệ thống
            TogglePlayerControls(false);

            // 2. Chạy Animation Chết
            if (_animator != null)
            {
                _animator.SetTrigger(_deathTriggerHash);
                
                // Đợi 1 frame để Animator chuyển sang State mới
                yield return null; 
                
                // Lấy độ dài clip thực tế từ State hiện tại (đã bao gồm Override)
                var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                float waitTime = (stateInfo.shortNameHash == _deathStateHash || stateInfo.fullPathHash == _deathStateHash) 
                    ? stateInfo.length 
                    : defaultDeathDelay;
                
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                yield return new WaitForSeconds(defaultDeathDelay);
            }

            // 3. Dịch chuyển và Reset Máu
            transform.position = RespawnPoint;
            if (_health != null)
                _health.SetMaxHealth(_health.MaxHealth, true);

            // 4. Chạy Animation Hồi Sinh
            if (_animator != null)
            {
                _animator.SetTrigger(_respawnTriggerHash);
                
                // Đợi animation hồi sinh diễn ra (ví dụ 1s)
                yield return new WaitForSeconds(1.0f); 
            }

            // 5. Mở khoá hệ thống
            TogglePlayerControls(true);

            _isRespawning = false;
        }

        private void TogglePlayerControls(bool state)
        {
            if (_input != null) _input.enabled = state;
            if (_movement != null)
            {
                if (!state) _movement.StopMovement();
                _movement.SetCanMove(state);
            }
        }
    }
}

