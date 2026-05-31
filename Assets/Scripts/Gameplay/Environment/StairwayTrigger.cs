using UnityEngine;

namespace Gameplay.Environment
{
    /// <summary>
    /// Giúp nhân vật di chuyển chéo khi đi qua khu vực cầu thang.
    /// Gắn script này vào các GameObject có Collider(IsTrigger=true) đặt tại cầu thang.
    /// </summary>
    public class StairwayTrigger : MonoBehaviour
    {
        [Header("Stair Direction")]
        [Tooltip("Hướng chéo của cầu thang (ví dụ: [1, 1] cho hướng Đông Bắc)")]
        [SerializeField] private Vector2 stairVector = new Vector2(1, 1);
        
        [Header("Settings")]
        [SerializeField] private float diagonalStrength = 0.5f;

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                var rb = other.GetComponent<Rigidbody2D>();
                if (rb == null) return;

                // Nếu người chơi đang có vận tốc trục Y (đi lên/xuống)
                if (Mathf.Abs(rb.velocity.y) > 0.1f)
                {
                    // Ép một phần vận tốc sang trục X theo hướng stairVector
                    float targetVX = Mathf.Sign(rb.velocity.y) * stairVector.x * Mathf.Abs(rb.velocity.y) * diagonalStrength;
                    
                    // Chỉ hỗ trợ nếu người chơi không chủ động bấm phím trái/phải quá gắt
                    if (Mathf.Abs(rb.velocity.x) < 0.5f)
                    {
                        rb.velocity = new Vector2(targetVX, rb.velocity.y);
                    }
                }
            }
        }
    }
}
