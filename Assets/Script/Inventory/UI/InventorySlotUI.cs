using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using Script.Inventory.Controller;

namespace Script.Inventory.UI
{
    public class InventorySlotUI : MonoBehaviour,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] public Image icon;
        [SerializeField] public TextMeshProUGUI amountText;
        [SerializeField] public Image durabilityBar;
        [SerializeField] public Image highlightImage;
        [SerializeField] public GameObject tooltipRoot;
        [SerializeField] public TextMeshProUGUI tooltipName;
        [SerializeField] public TextMeshProUGUI tooltipDesc;

        private InventorySlot _slotData;
        private ItemActionMenu _actionMenu;
        public int SlotIndex { get; private set; }
        public void Init(int index, ItemActionMenu menu)
        {
            SlotIndex = index;
            _actionMenu = menu;
        }
        public void Refresh(InventorySlot slotData)
        {
            _slotData = slotData;

            if (_slotData == null || _slotData.IsEmpty)
            {
                ClearVisuals();
                return;
            }
            
            icon.sprite = _slotData.Item.Icon;
            icon.enabled = true;
            
            if (_slotData.Amount > 1)
            {
                amountText.text = _slotData.Amount.ToString();
                amountText.enabled = true;
            }
            else
            {
                amountText.enabled = false;
            }
            
            var showDur = _slotData.IsEquipment;
            durabilityBar.gameObject.SetActive(showDur);
            if (showDur)
                durabilityBar.fillAmount = _slotData.DurabilityPercent;
        }

        private void ClearVisuals()
        {
            icon.sprite = null;
            icon.enabled = false;
            amountText.enabled = false;
            if (durabilityBar != null) durabilityBar.gameObject.SetActive(false);
            HideTooltip();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            HideTooltip();
            if (eventData.button == PointerEventData.InputButton.Right && _slotData is { IsEmpty: false } && _actionMenu != null)
                _actionMenu.ShowMenu(_slotData, transform.position);
            else if (_actionMenu != null)
                _actionMenu.HideMenu();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            highlightImage.enabled = true;
            if (_slotData is { IsEmpty: false }) ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            highlightImage.enabled = false;
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
            if (tooltipRoot != null) tooltipRoot.SetActive(false);
        }
    }
}