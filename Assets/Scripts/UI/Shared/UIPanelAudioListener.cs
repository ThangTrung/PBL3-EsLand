using UnityEngine;
using EsLand.Data.Audio;
using EsLand.Infrastructure.Audio;
using UI.Inventory;

namespace EsLand.UI.Shared
{
    /// <summary>
    /// Phát âm thanh khi một Panel UI (như Inventory) được mở ra.
    /// </summary>
    public class UIPanelAudioListener : MonoBehaviour
    {
        [SerializeField] private AudioData _openSound;
        private InventoryPanelUI _panel;

        private void Awake()
        {
            _panel = GetComponent<InventoryPanelUI>();
        }

        private void OnEnable()
        {
            // Lưu ý: InventoryPanelUI của bạn dùng SetActive(visible) 
            // nên chúng ta có thể tận dụng OnEnable của chính GameObject này hoặc của CanvasRoot.
            if (AudioManager.Instance != null && _openSound != null)
            {
                AudioManager.Instance.PlaySFX(_openSound);
            }
        }
    }
}
