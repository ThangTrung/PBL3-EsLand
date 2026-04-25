using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Script.Inventory.Controller;
using Script.Interfaces;

namespace Script.Inventory.UI
{
    public class InventorySlotUI : MonoBehaviour,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IInventorySlotUI
    {
        [Header("References")]
        [SerializeField] public Image icon;
        [SerializeField] public TextMeshProUGUI amountText;
        [SerializeField] public Image durabilityBar;
        [SerializeField] public Image highlightImage;
        [SerializeField] public Image equippedOverlay; // Added this
        [SerializeField] public GameObject tooltipRoot;
        [SerializeField] public TextMeshProUGUI tooltipName;
        [SerializeField] public TextMeshProUGUI tooltipDesc;

        [Header("Actions")]
        [SerializeField] public MonoBehaviour actionMenuProvider;
        
        private IInventorySlot _slotData;
        public int SlotIndex { get; private set; }

        public event Action<IInventorySlot, Vector3> OnRightClicked;
        public event Action<int> OnHovered;

        public void Init(int index)
        {
            SlotIndex = index;
        }

        public void Refresh(IInventorySlot slotData)
        {
            _slotData = slotData;

            if (_slotData == null || _slotData.IsEmpty)
            {
                ClearVisuals();
                return;
            }
            
            icon.sprite = _slotData.Item.Icon;
            icon.enabled = true;
            
            amountText.enabled = _slotData.Amount > 1;
            if (_slotData.Amount > 1)
            {
                amountText.text = _slotData.Amount.ToString();
            }
            
            var hasDurability = _slotData.Item is IDurable;
            if(durabilityBar) durabilityBar.gameObject.SetActive(hasDurability);
            if (hasDurability && durabilityBar)
                durabilityBar.fillAmount = _slotData.DurabilityPercent;

            // Check if equipped to show overlay
            if (!equippedOverlay) return;
            var isEquipped = false;
            // This is a bit of a shortcut, ideally we'd pass this info or have a cleaner way
            // to access the handler from here, but for UI refresh it's practical.
            if (InventoryController.Instance && InventoryController.Instance.ActionHandler != null)
            {
                isEquipped = InventoryController.Instance.ActionHandler.IsEquipped(_slotData);
            }
            equippedOverlay.enabled = isEquipped;
        }

        private void ClearVisuals()
        {
            icon.sprite = null;
            icon.enabled = false;
            amountText.enabled = false;
            if (durabilityBar) durabilityBar.gameObject.SetActive(false);
            if (equippedOverlay) equippedOverlay.enabled = false;
            HideTooltip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    break;
                case PointerEventData.InputButton.Right:
                {
                    HideTooltip();
                    if (_slotData == null || _slotData.IsEmpty) return;
                    var worldPos = eventData.position;
                    OnRightClicked?.Invoke(_slotData, worldPos);
                    break;
                }
                case PointerEventData.InputButton.Middle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetHighlight(bool active)
        {
            highlightImage.enabled = active;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHovered?.Invoke(SlotIndex);
            if (_slotData is { IsEmpty: false }) ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        private void ShowTooltip()
        {
            if (tooltipRoot == null || _slotData == null) return;
            tooltipRoot.SetActive(true);
            if (tooltipName != null) tooltipName.text = _slotData.Item.ItemName;
            if (tooltipDesc != null) tooltipDesc.text = _slotData.Item.Description;
        }

        private void HideTooltip()
        {
            if (tooltipRoot) tooltipRoot.SetActive(false);
        }
        public void ResetState()
        {
            if (highlightImage != null) highlightImage.enabled = false;
            HideTooltip();
        }
    }
}