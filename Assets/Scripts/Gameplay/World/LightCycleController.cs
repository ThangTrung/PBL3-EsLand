using UnityEngine;
using UnityEngine.Rendering.Universal;
using Core;

namespace Gameplay.World
{
    /// <summary>
    /// Điều khiển ánh sáng Global dựa trên thời gian từ TimeManager.
    /// </summary>
    public class LightCycleController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Light2D globalLight;

        [Header("Cycle Settings")]
        [SerializeField] private Gradient cycleGradient;
        [SerializeField] private AnimationCurve intensityCurve;

        private void Start()
        {
            if (globalLight == null)
            {
                globalLight = GetComponent<Light2D>();
            }

            // Thiết lập giá trị mặc định nếu chưa có
            if (intensityCurve.length == 0)
            {
                intensityCurve.AddKey(0f, 0.2f);   // Đêm (0h)
                intensityCurve.AddKey(6f, 0.5f);   // Bình minh
                intensityCurve.AddKey(12f, 1.0f);  // Trưa
                intensityCurve.AddKey(18f, 0.5f);  // Hoàng hôn
                intensityCurve.AddKey(24f, 0.2f);  // Đêm (24h)
            }
        }

        private void Update()
        {
            if (globalLight == null || TimeManager.Instance == null) return;

            float time = TimeManager.Instance.CurrentTime;
            
            // Lấy tỉ lệ 0-1 cho Gradient (24h)
            float t = time / 24f;

            // Cập nhật màu sắc
            if (cycleGradient != null)
            {
                globalLight.color = cycleGradient.Evaluate(t);
            }

            // Cập nhật cường độ
            globalLight.intensity = intensityCurve.Evaluate(time);
        }
    }
}
