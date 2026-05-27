using Core.Contracts.Shared;
using UnityEngine;

namespace Gameplay.Environment
{
    /// <summary>
    /// Thành phần gắn vào các GameObject để xác định tầng của chúng.
    /// </summary>
    public class LayerEntity : MonoBehaviour, ILayerable
    {
        [SerializeField] private int initialLayer = 0;
        
        public int CurrentLayer { get; private set; }
        public System.Action<int> OnLayerChanged { get; set; }

        private void Awake()
        {
            CurrentLayer = initialLayer;
        }

        public void SetLayer(int newLayer)
        {
            if (CurrentLayer == newLayer) return;
            
            CurrentLayer = newLayer;
            OnLayerChanged?.Invoke(CurrentLayer);
            
            // Tùy chọn: Cập nhật Sorting Layer của SpriteRenderer để hiển thị đúng độ cao
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // Ví dụ: Mỗi tầng cách nhau 1000 sorting order
                spriteRenderer.sortingOrder = (CurrentLayer * 1000) + (int)(transform.position.y * -10);
            }
        }
    }
}
