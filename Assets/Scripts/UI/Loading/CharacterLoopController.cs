using UnityEngine;

namespace UI.Loading
{
    /// <summary>
    /// Điều khiển nhân vật chạy qua lại màn hình lặp đi lặp lại.
    /// </summary>
    public class CharacterLoopController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float speed = 500f; // Tốc độ chạy (tính theo tọa độ UI nếu dùng Canvas)
        [SerializeField] private float padding = 100f; // Khoảng cách dừng trước khi quay đầu từ biên màn hình

        private RectTransform _rectTransform;
        private Canvas _canvas;
        private int _direction = 1; // 1: Phải, -1: Trái

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
        }

        private void Update()
        {
            if (_canvas == null) return;

            // Tính toán giới hạn màn hình dựa trên Canvas
            float screenWidth = _canvas.GetComponent<RectTransform>().rect.width;
            float leftBound = -screenWidth / 2 + padding;
            float rightBound = screenWidth / 2 - padding;

            // Di chuyển
            float moveStep = speed * _direction * Time.deltaTime;
            _rectTransform.anchoredPosition += new Vector2(moveStep, 0);

            // Kiểm tra va chạm biên
            if (_direction == 1 && _rectTransform.anchoredPosition.x >= rightBound)
            {
                Flip();
            }
            else if (_direction == -1 && _rectTransform.anchoredPosition.x <= leftBound)
            {
                Flip();
            }
        }

        private void Flip()
        {
            _direction *= -1;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
}
