using System.Collections.Generic;
using UnityEngine;

namespace Script.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject canvasRoot;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private ItemActionMenu actionMenu;

        private InventoryController _inventoryController;
        private readonly List<InventorySlotUI> _slotUIs = new List<InventorySlotUI>();

        public void Setup(InventoryController controller)
        {
            _inventoryController = controller;
            
            if (actionMenu != null)
                actionMenu.Setup(_inventoryController);

            // Lắng nghe event mở/đóng và thay đổi dữ liệu
            if (_inventoryController != null)
            {
                _inventoryController.OnInventoryChanged += RefreshUI;
                _inventoryController.OnVisibilityChanged += SetVisible;
            }

            // Tạo sẵn các ô UI
            BuildSlots();

            // Ẩn lúc đầu
            if (canvasRoot != null)
                canvasRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_inventoryController == null) 
                return;
            _inventoryController.OnInventoryChanged -= RefreshUI;
            _inventoryController.OnVisibilityChanged -= SetVisible;
        }

        // ── Build ─────────────────────────────────────────────────

        private void BuildSlots()
        {
            if (slotPrefab == null || slotsContainer == null) return;

            // Xóa slot cũ (nếu có)
            foreach (Transform child in slotsContainer)
                Destroy(child.gameObject);
            _slotUIs.Clear();

            int cap = _inventoryController != null ? _inventoryController.Capacity : 20;
            for (int i = 0; i < cap; i++)
            {
                var go = Instantiate(slotPrefab, slotsContainer);
                var slotUI = go.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.Init(i, actionMenu);
                    _slotUIs.Add(slotUI);
                }
            }

            RefreshUI();
        }

        // ── Refresh ───────────────────────────────────────────────

        private void RefreshUI()
        {
            if (_inventoryController == null) return;
            var slots = _inventoryController.Slots;

            for (var i = 0; i < _slotUIs.Count; i++)
            {
                var data = (i < slots.Count) ? slots[i] : null;
                _slotUIs[i].Refresh(data);
            }
        }

        // ── Visibility ────────────────────────────────────────────

        private void SetVisible(bool visible)
        {
            if (canvasRoot != null)
                canvasRoot.SetActive(visible);

            // Đóng action menu khi đóng inventory
            if (!visible && actionMenu != null)
                actionMenu.HideMenu();
        }
    }
}