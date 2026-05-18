using System;
using UnityEngine;
using System.Collections;

namespace Infrastructure.Visuals
{
    public interface IVisualEffect
    {
        void PlayEffect();
        void StopEffect();
    }

    public class FadeBehind : MonoBehaviour, IVisualEffect
    {
        [Header("Cài đặt làm mờ")]
        [SerializeField] private float fadeAlpha = 0.4f;
        [SerializeField] private float fadeSpeed = 0.2f;

        private SpriteRenderer _spriteRenderer;
        private Color _originalColor;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null)
                _originalColor = _spriteRenderer.color;
        }

        public void PlayEffect()
        {
            if (!gameObject.activeInHierarchy) return;
            TransitionTo(fadeAlpha);
        }

        public void StopEffect()
        {
            if (!gameObject.activeInHierarchy) return;
            TransitionTo(_originalColor.a);
        }

        private void TransitionTo(float targetAlpha)
        {
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha));
        }

        private IEnumerator FadeRoutine(float targetAlpha)
        {
            if (_spriteRenderer == null) yield break;

            var currentColor = _spriteRenderer.color;
            var startAlpha = currentColor.a;
            var timer = 0f;

            while (timer < fadeSpeed)
            {
                timer += Time.deltaTime;
                var newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeSpeed);
                _spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
                yield return null;
            }

            _spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);
        }

        // Logic trigger để riêng hoặc có thể tách ra class khác
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player")) PlayEffect();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player")) StopEffect();
        }
    }
}
