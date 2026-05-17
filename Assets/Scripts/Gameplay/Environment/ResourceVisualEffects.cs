using System.Collections;
using Gameplay.World;
using UnityEngine;

namespace Gameplay.Environment
{
    /// <summary>
    /// Component xử lý các hiệu ứng hình ảnh (Visual Effects) cho tài nguyên.
    /// Lắng nghe event từ ResourceNode để thực hiện rung lắc (Shake).
    /// Tuân thủ nguyên lý SOLID: Tách biệt Logic và View.
    /// </summary>
    public class ResourceVisualEffects : MonoBehaviour
    {
        [Header("Shake Settings")]
        [SerializeField] private float shakeDuration = 0.2f;
        [SerializeField] private float shakeMagnitude = 0.1f;
        [SerializeField] private float restoreSpeed = 5f;

        private ResourceNode _resourceNode;
        private Vector3 _originalLocalPosition;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            _resourceNode = GetComponent<ResourceNode>();
            _originalLocalPosition = transform.localPosition;
        }

        private void OnEnable()
        {
            if (_resourceNode != null)
            {
                _resourceNode.OnDamaged += PlayShakeEffect;
            }
        }

        private void OnDisable()
        {
            if (_resourceNode != null)
            {
                _resourceNode.OnDamaged -= PlayShakeEffect;
            }
        }

        private void PlayShakeEffect()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
            }
            _shakeCoroutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                // Tạo độ lệch ngẫu nhiên
                float x = Random.Range(-1f, 1f) * shakeMagnitude;
                float y = Random.Range(-1f, 1f) * shakeMagnitude;

                transform.localPosition = _originalLocalPosition + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Trả về vị trí cũ một cách mượt mà
            float t = 0;
            Vector3 currentPos = transform.localPosition;
            while (t < 1f)
            {
                t += Time.deltaTime * restoreSpeed;
                transform.localPosition = Vector3.Lerp(currentPos, _originalLocalPosition, t);
                yield return null;
            }
            
            transform.localPosition = _originalLocalPosition;
            _shakeCoroutine = null;
        }
    }
}
