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

        public bool IsVisible => canvasRoot.activeSelf;

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
        
        public void SetVisible(bool visible)
        {
            if (!visible) 
            {
                actionMenu.HideMenu();
                foreach (var slot in _slotUIs)
                    slot.ResetState();
                
                if (UnityEngine.EventSystems.EventSystem.current)
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }
            canvasRoot.SetActive(visible);
            if (visible)
                RefreshUI();
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

            var slots = inventoryController.Slots;

            // Báo ra Console để biết hàm này đã thực sự chạy
            Debug.Log($"[TÚI ĐỒ] Đang cập nhật UI... Tổng số ô chứa đồ: {slots.Count}");

            for (var i = 0; i < _slotUIs.Count; i++)
            {
                var data = (i < slots.Count) ? slots[i] : null;

                // Log ra xem UI có nhận được đúng món đồ không 
                if (data != null)
                {
                    Debug.Log($"[TÚI ĐỒ] Ô số {i} đang vẽ món: {data.Item.ItemName} | Số lượng: {data.Amount}");
                }
                _slotUIs[i].Refresh(data);
            }
        }
    }
}