using System.Collections;
using UnityEngine;

namespace Gameplay.World
{
    /// <summary>
    /// Hiệu ứng rung màn hình khi nhân vật nhận sát thương hoặc có chấn động.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private Vector3 _originalPos;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            _originalPos = transform.localPosition;
        }

        public void Shake(float duration, float magnitude)
        {
            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
        }

        private IEnumerator DoShake(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = new Vector3(_originalPos.x + x, _originalPos.y + y, _originalPos.z);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = _originalPos;
            _shakeCoroutine = null;
        }
    }
}
