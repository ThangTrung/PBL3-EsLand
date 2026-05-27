using Core.Contracts.Shared;
using UnityEngine;

namespace Gameplay.Environment
{
    /// <summary>
    /// Trigger dùng để chuyển tầng cho thực thể khi đi qua (ví dụ: cầu thang, cửa).
    /// </summary>
    public class LayerTrigger : MonoBehaviour
    {
        [SerializeField] private int targetLayer = 0;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Kiểm tra nếu đối tượng đi vào có ILayerable
            if (other.TryGetComponent<ILayerable>(out var layerable))
            {
                layerable.SetLayer(targetLayer);
                Debug.Log($"{other.name} đã đi vào Trigger và chuyển sang Layer {targetLayer}");
            }
        }
    }
}
