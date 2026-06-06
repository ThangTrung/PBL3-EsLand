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
    public class InventorySlotUI : SlotUIBase, IPointerClickHandler
    {
        [Header("Inventory Specific")]
        [SerializeField] private Image durabilityBar;
        [SerializeField] private Image equippedOverlay;

        private IInventorySlot _slotData;
        private IItemActionHandler _actionHandler;
        private Image _slotFrame;
        private int SlotIndex { get; set; }

        public event Action<IActionableItem, Vector3> OnRightClicked;
        public event Action<int, IInventorySlot> OnLeftClicked;

        public void Init(int index, IItemActionHandler actionHandler = null)
        {
            SlotIndex = index;
            _actionHandler = actionHandler;
            _slotFrame = GetComponent<Image>();
        }

        public void Refresh(IInventorySlot slotData)
        {
            _slotData = slotData;

            if (_slotFrame) _slotFrame.enabled = true;
            
            if (_slotData == null || _slotData.IsEmpty)
            {
                ClearVisuals();
                return;
            }

            _hasData = true;
            _cachedTitle = _slotData.ItemData.ItemName;
            _cachedContent = _slotData.ItemData.Description;

            if (iconImage)
            {
                iconImage.sprite = _slotData.ItemData.Icon;
                iconImage.enabled = _slotData.ItemData.Icon;
            }

            if (amountText)
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

            if (equippedOverlay)
            {
                var isEquipped = _actionHandler?.IsEquipped(_slotData) ?? false;
                equippedOverlay.enabled = isEquipped;
            }
        }

        protected override void ClearVisuals()
        {
            base.ClearVisuals();
            if (durabilityBar) durabilityBar.gameObject.SetActive(false);
            if (equippedOverlay) equippedOverlay.enabled = false;
        }

        public void OnPointerClick(PointerEventData eventData)  
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    if (_slotData is { IsEmpty: false })
                    {
                        OnLeftClicked?.Invoke(SlotIndex, _slotData);
                    }
                    break;
                case PointerEventData.InputButton.Right:
                {
                    if (GlobalTooltipUI.Instance != null) GlobalTooltipUI.Instance.Hide();
                    
                    if (_slotData == null || _slotData.IsEmpty) return;
                    var context = new InventorySlotActionContext(_slotData, _actionHandler);
                    OnRightClicked?.Invoke(context, eventData.position);
                    break;
                }
            }
        }

        public void ResetState()
        {
            SetHighlight(false);
            if (GlobalTooltipUI.Instance != null) GlobalTooltipUI.Instance.Hide();
        }
    }
}
