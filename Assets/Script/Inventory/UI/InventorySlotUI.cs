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

        [Header("Actions")]
        [SerializeField] public ItemActionMenu actionMenu;
        
        private InventorySlot _slotData;
        public int SlotIndex { get; private set; }

        public event Action<InventorySlot, Vector3> OnRightClicked;

        public void Init(int index)
        {
            SlotIndex = index;
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
            
            amountText.enabled = _slotData.Amount > 1;
            if (_slotData.Amount > 1)
            {
                amountText.text = _slotData.Amount.ToString();
            }
            
            var showDur = _slotData.IsEquipment;
            if(durabilityBar) durabilityBar.gameObject.SetActive(showDur);
            if (showDur && durabilityBar)
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
            if (eventData.button != PointerEventData.InputButton.Right || _slotData is not { IsEmpty: false }) 
                return;
            var worldPos = eventData.position;
            OnRightClicked?.Invoke(_slotData, worldPos);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(highlightImage) highlightImage.enabled = true;
            if (_slotData is { IsEmpty: false }) ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(highlightImage) highlightImage.enabled = false;
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
        public void ResetState()
        {
            if (highlightImage != null) highlightImage.enabled = false;
            HideTooltip();
        }
    }
}