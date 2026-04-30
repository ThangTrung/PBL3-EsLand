using System.Collections.Generic;
using System.Linq;
using Script.Entities;
using Script.Equipment.Interfaces;
using Script.Items;
using Script.Shared.Interfaces;
using UnityEngine;

namespace Script.Equipment.UI
{
    public class EquipmentPanelUI : MonoBehaviour
    {
        [Header("UI")]
        public GameObject equipmentPanel;

        [Header("Slots")]
        [SerializeField] private List<EquipmentSlotUI> slots = new List<EquipmentSlotUI>();

        private IInventoryHolder _provider;
        private IEquipmentManager _equipmentManager;

        public void Initialize(IInventoryHolder provider)
        {
            if (provider == null) return;
            _provider = provider;
            _equipmentManager = provider.EquipmentManager;

            if (_equipmentManager != null)
            {
                _equipmentManager.OnItemEquipped += HandleItemEquipped;
                _equipmentManager.OnItemUnequipped += HandleItemUnequipped;

                foreach (var pair in _equipmentManager.EquippedItems)
                    HandleItemEquipped(pair.Key, pair.Value);
            }

            foreach (var slot in slots.Where(slot => slot != null))
            {
                slot.SetEquipmentManager(_equipmentManager);
                slot.OnRightClicked += HandleSlotRightClicked;
            }
        }

        private void Awake()
        {
            if (equipmentPanel == null) equipmentPanel = transform.Find("EquipmentPanel")?.gameObject;
            if (equipmentPanel != null) equipmentPanel.SetActive(false);
        }

        public void ToggleUI()
        {
            var isCurrentlyVisible = equipmentPanel && equipmentPanel.activeSelf;
            SetVisible(!isCurrentlyVisible);
        }

        private void SetVisible(bool visible)
        {
            if (equipmentPanel) equipmentPanel.SetActive(visible);

            if (_provider is Player player)
                player.SetUIState(player.IsInventoryOpenInternal, visible);

            if (visible) return;
            foreach (var slot in slots.Where(slot => slot))
                slot.SetHighlight(false);
        }

        private static void HandleSlotRightClicked(IActionableItem context, Vector3 screenPos)
        {
            context?.Unequip();
        }

        private void HandleItemEquipped(EquipSlot slot, IEquippable equippableItem)
        {
            var slotUI = FindSlotUI(slot);
            if (slotUI == null) return;

            Sprite icon = null;
            if (equippableItem is Items.Equipment item) icon = item.Icon;

            slotUI.SetItem(equippableItem, icon);
        }

        private void HandleItemUnequipped(EquipSlot slot, IEquippable item)
        {
            var slotUI = FindSlotUI(slot);
            if (slotUI != null) slotUI.ClearItem();
        }

        private EquipmentSlotUI FindSlotUI(EquipSlot slot)
        {
            return slots.FirstOrDefault(s => s != null && s.SlotType == slot);
        }
    }
}