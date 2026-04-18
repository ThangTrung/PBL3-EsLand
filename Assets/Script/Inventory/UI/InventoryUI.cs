using System.Collections.Generic;
using UnityEngine;
using Script.Inventory.Controller;

namespace Script.Inventory.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private InventoryController inventoryController;
        [SerializeField] private ItemActionMenu actionMenu;

        [Header("UI References")]
        [SerializeField] private GameObject canvasRoot;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;

        private readonly List<InventorySlotUI> _slotUIs = new List<InventorySlotUI>();

        public bool IsVisible => canvasRoot != null && canvasRoot.activeSelf;

        private void Start()
        {
            inventoryController.OnInventoryChanged += RefreshUI;
            BuildSlots();
            SetVisible(false); 
        }

        private void OnDestroy()
        {
            inventoryController.OnInventoryChanged -= RefreshUI;
        }

        // Gọi hàm này từ phím tắt (ví dụ nhấn nút Tab hoặc I)
        public void SetVisible(bool visible)
        {
            if (canvasRoot != null)
                canvasRoot.SetActive(visible);

            switch (visible)
            {
                case false when actionMenu != null:
                    actionMenu.HideMenu();
                    break;
                case true:
                    RefreshUI();
                    break;
            }
        }

        private void BuildSlots()
        {
            foreach (Transform child in slotsContainer)
                Destroy(child.gameObject);
            _slotUIs.Clear();

            var cap = inventoryController.Capacity;
            for (var i = 0; i < cap; i++)
            {
                var go = Instantiate(slotPrefab, slotsContainer);
                var slotUI = go.GetComponent<InventorySlotUI>();
                if (slotUI == null) 
                    continue;
                slotUI.Init(i);
                slotUI.OnRightClicked += HandleSlotRightClicked;
                _slotUIs.Add(slotUI);
            }
        }

        private void HandleSlotRightClicked(InventorySlot slotData, Vector3 pos)
        {
            Debug.Log("HandleSlotRightClicked");
            actionMenu.ShowMenu(slotData, pos);
        }

        private void RefreshUI()
        {
            if (inventoryController == null || !IsVisible) return;
            var slots = inventoryController.Slots;

            for (var i = 0; i < _slotUIs.Count; i++)
            {
                var data = (i < slots.Count) ? slots[i] : null;
                _slotUIs[i].Refresh(data);
            }
        }
    }
}