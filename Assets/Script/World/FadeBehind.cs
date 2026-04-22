using UnityEngine;
using System.Collections;

public class FadeBehind : MonoBehaviour
{
    [Header("Cài đặt làm mờ")]
    [SerializeField] private float fadeAlpha = 0.4f; // Độ mờ (0 là tàng hình, 1 là rõ)
    [SerializeField] private float fadeSpeed = 0.2f; // Tốc độ mờ (giây)

    private SpriteRenderer _spriteRenderer;
    private Color _originalColor;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalColor = _spriteRenderer.color; // Lưu lại màu gốc
    }

    // Khi Player bước vào tán cây
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // THÊM DÒNG NÀY: Chỉ chạy hiệu ứng nếu cái cây đang được BẬT
            if (!gameObject.activeInHierarchy) return;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(fadeAlpha));
        }
    }

    // Khi Player đi ra khỏi tán cây
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // THÊM DÒNG NÀY: Chỉ chạy hiệu ứng nếu cái cây đang được BẬT
            if (!gameObject.activeInHierarchy) return;

            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(_originalColor.a));
        }
    }

    // Vòng lặp làm mờ từ từ
    private IEnumerator FadeRoutine(float targetAlpha)
    {
        Color currentColor = _spriteRenderer.color;
        float startAlpha = currentColor.a;
        float timer = 0f;

        while (timer < fadeSpeed)
        {
            timer += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeSpeed);
            _spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            yield return null;
        }

        // Đảm bảo số chốt cuối cùng chính xác
        _spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
    }
}