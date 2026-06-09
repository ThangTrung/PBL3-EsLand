using UnityEngine;
using Core.Contracts.Environment;

namespace Gameplay.Environment
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class OneWayBarrier : MonoBehaviour, IArenaBarrier
    {
        private BoxCollider2D _collider;
        private bool _isLocked = false;

        [Tooltip("Lực đẩy nhẹ để ngăn Player dùng Dash xuyên qua tường")]
        [SerializeField] private float failsafePushForce = 15f;

        private void Awake()
        {
            // Tìm BoxCollider2D vật lý (không phải trigger)
            var colliders = GetComponents<BoxCollider2D>();
            foreach (var col in colliders)
            {
                if (!col.isTrigger)
                {
                    _collider = col;
                    break;
                }
            }
        }

        public void Lock()
        {
            _isLocked = true;
            if (_collider != null)
            {
                _collider.enabled = true;
            }
        }

        public void Unlock()
        {
            _isLocked = false;
            if (_collider != null)
            {
                _collider.enabled = false;
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            // Chỉ kích hoạt failsafe khi rào đang đóng và đối tượng là Player
            if (!_isLocked || !collision.CompareTag("Player")) return;

            if (collision.TryGetComponent<Rigidbody2D>(out var rb))
            {
                // Lấy tọa độ trung tâm Arena trực tiếp từ parent (Hỗ trợ Dynamic Spawning)
                Vector3 arenaCenter = transform.parent != null ? transform.parent.position : transform.position;

                // Tính toán hướng đẩy: Từ vị trí hiện tại của Player đẩy về hướng Arena Center
                Vector2 pushDirection = (arenaCenter - collision.transform.position).normalized;
                
                // Áp dụng lực đẩy (Knockback an toàn) liên tục chừng nào Player còn kẹt trong vùng Trigger
                rb.AddForce(pushDirection * failsafePushForce, ForceMode2D.Force);
            }
        }
    }
}