using UnityEngine;
using UnityEngine.EventSystems;
using EsLand.Data.Audio;
using EsLand.Infrastructure.Audio;

namespace EsLand.UI.Shared
{
    /// <summary>
    /// Gắn vào bất kỳ UI Element nào (Button, Icon) để phát tiếng Click và Hover.
    /// </summary>
    public class UIAudioHandler : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField] private AudioData _clickSound;
        [SerializeField] private AudioData _hoverSound;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_clickSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_clickSound);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_hoverSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_hoverSound);
            }
        }
    }
}
