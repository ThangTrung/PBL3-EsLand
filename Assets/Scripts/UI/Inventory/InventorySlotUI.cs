using System;
using Core.Contracts.Inventory;
using Core.Contracts.Shared;
using TMPro;
using UI.ItemActions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Inventory
{
    public class InventorySlotUI : MonoBehaviour,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] public Image icon;
        [SerializeField] public TextMeshProUGUI amountText;
        [SerializeField] public Image durabilityBar;
        [SerializeField] public Image highlightImage;
        [SerializeField] public Image equippedOverlay;
        [SerializeField] public GameObject tooltipRoot;
        [SerializeField] public TextMeshProUGUI tooltipName;
        [SerializeField] public TextMeshProUGUI tooltipDesc;

        private IInventorySlot _slotData;
        private IItemActionHandler _actionHandler;
        private int SlotIndex { get; set; }

        public event Action<IActionableItem, Vector3> OnRightClicked;
        public void Init(int index, IItemActionHandler actionHandler = null)
        {
            SlotIndex = index;
            _actionHandler = actionHandler;
        }

        public void Refresh(IInventorySlot slotData)
        {
            _slotData = slotData;

            if (GetComponent<Image>() != null) 
                GetComponent<Image>().enabled = true;
            
            if (_slotData == null || _slotData.IsEmpty)
            {
                ClearVisuals();
                return;
            }

            if (icon != null)
            {
                icon.sprite = _slotData.ItemData.Icon;
                icon.enabled = _slotData.ItemData.Icon != null;
            }

            if (amountText != null)
            {
                amountText.enabled = _slotData.Amount > 1;
                if (_slotData.Amount > 1)
                    amountText.text = _slotData.Amount.ToString();
            }

            var hasDurability = _slotData.ItemData is IDurable;
            if (durabilityBar) 
            {
                durabilityBar.gameObject.SetActive(hasDurability);
                if (hasDurability)
                    durabilityBar.fillAmount = _slotData.DurabilityPercent;
            }

            if (equippedOverlay != null)
            {
                var isEquipped = _actionHandler?.IsEquipped(_slotData) ?? false;
                equippedOverlay.enabled = isEquipped;
            }
        }

        private void ClearVisuals()
        {
            if (icon != null)
            {
                icon.sprite = null;
                icon.enabled = false;
            }
            
            if (amountText != null) amountText.enabled = false;
            if (durabilityBar != null) durabilityBar.gameObject.SetActive(false);
            if (equippedOverlay != null) equippedOverlay.enabled = false;
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
                    Debug.Log($"[InventorySlotUI] Right-clicked on slot {SlotIndex} with item: {(_slotData?.ItemData?.ItemName ?? "Empty")}");
                    if (_slotData == null || _slotData.IsEmpty) return;
                    var context = new InventorySlotActionContext(_slotData, _actionHandler);
                    OnRightClicked?.Invoke(context, eventData.position);
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
            SetHighlight(true);
            if (_slotData is { IsEmpty: false }) ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHighlight(false);
            HideTooltip();
        }

        private void ShowTooltip()
        {
            if (tooltipRoot == null || _slotData == null) return;
            tooltipRoot.SetActive(true);
            if (tooltipName != null) tooltipName.text = _slotData.ItemData.ItemName;
            if (tooltipDesc != null) tooltipDesc.text = _slotData.ItemData.Description;
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




