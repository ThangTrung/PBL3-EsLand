using UnityEngine;
using UnityEngine.UI;

using Script.Inventory.Controller;

namespace Script.Inventory.Controller
{
    public class ItemActionMenu : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject menuPanel;

        [Header("Buttons")]
        [SerializeField] private Button useButton;
        [SerializeField] private Button dropButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button unequipButton;

        private InventorySlot _currentSlot;
        private InventoryController _inventoryController;
        //private Character _user;

        private void Awake()
        {
            if (menuPanel != null) 
                menuPanel.SetActive(false);
        }
        
        public void Setup(InventoryController controller)
        {
            _inventoryController = controller;
        }

        //public void SetUser(Character user) => _user = user;

        public void ShowMenu(InventorySlot slotData, Vector3 worldPos)
        {
            if (slotData == null || slotData.IsEmpty) 
                return;
            
            _currentSlot = slotData;
            var isEquip = slotData.IsEquipment;

            if (useButton != null) useButton.gameObject.SetActive(!isEquip);
            if (equipButton != null) equipButton.gameObject.SetActive(isEquip);
            if (unequipButton != null) unequipButton.gameObject.SetActive(isEquip);

            menuPanel.transform.position = worldPos;
            menuPanel.SetActive(true);
        }

        public void HideMenu()
        {
            if (menuPanel) 
                menuPanel.SetActive(false);
            _currentSlot = null;
        }
        
        // private Character GetUser()
        // {
        //     if (_user != null) return _user;
        //     return _inventoryController != null ? _inventoryController.GetComponentInParent<Character>() : null;
        // }

        // public void OnClickUse()
        // {
        //     var user = GetUser();
        //     if (_currentSlot == null || user == null || _inventoryController == null) return;
        //     _inventoryController.UseItem(_currentSlot, user);
        //     HideMenu();
        // }

        // public void OnClickEquip()
        // {
        //     var user = GetUser();
        //     if (_currentSlot == null || user == null) return;
        //     if (_currentSlot.Item is Items.Equipment equip)
        //         user.Equip(equip);
        //     HideMenu();
        // }

        // public void OnClickUnequip()
        // {
        //     var user = GetUser();
        //     if (_currentSlot == null || user == null) return;
        //     if (_currentSlot.Item is Items.Equipment equip)
        //         user.Unequip(equip);
        //     HideMenu();
        // }

        public void OnClickDrop()
        {
            if (_currentSlot == null || _inventoryController == null) return;
            _inventoryController.RemoveSlot(_currentSlot);
            HideMenu();
        }

        private void Update()
        {
            if (menuPanel && menuPanel.activeSelf &&
                Input.GetMouseButtonDown(0) &&
                !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                HideMenu();
            }
        }
    }
}