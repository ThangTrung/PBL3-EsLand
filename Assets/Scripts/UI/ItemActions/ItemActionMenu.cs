using Core.Contracts.Shared;
using UnityEngine;

namespace UI.ItemActions
{
    /// <summary>
    /// Shared action menu - decoupled from Inventory and Equipment.
    /// Accepts any IActionableItem context (InventorySlotActionContext or EquipmentSlotActionContext).
    /// </summary>
    public class ItemActionMenu : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ItemActionMenuUI menuUI;

        private IActionableItem _current;

        private void Awake()
        {
            if (menuUI == null) menuUI = GetComponentInChildren<ItemActionMenuUI>(true);
        }

        private void OnEnable()
        {
            if (menuUI == null) return;
            menuUI.OnUseClicked     += HandleUse;
            menuUI.OnDropClicked    += HandleDrop;
            menuUI.OnEquipClicked   += HandleEquip;
            menuUI.OnUnequipClicked += HandleUnequip;
        }

        private void OnDisable()
        {
            if (menuUI == null) return;
            menuUI.OnUseClicked     -= HandleUse;
            menuUI.OnDropClicked    -= HandleDrop;
            menuUI.OnEquipClicked   -= HandleEquip;
            menuUI.OnUnequipClicked -= HandleUnequip;
        }

        public void ShowMenu(IActionableItem item, Vector3 screenPos)
        {
            _current = item;
            if (_current == null) return;
            Debug.Log($"[ItemActionMenu] Showing menu for item: {_current}. CanUse: {_current.CanUse}, CanEquip: {_current.CanEquip}, CanUnequip: {_current.CanUnequip}");
            menuUI.Show(screenPos, _current.CanUse, _current.CanEquip, _current.CanUnequip);
        }

        public void HideMenu()
        {
            Debug.Log("[ItemActionMenu] Hiding menu.");
            _current = null;
            if (menuUI) menuUI.Hide();
        }

        private void HandleUse()
        {
            Debug.Log($"[ItemActionMenu] HandleUse called for item: {_current}");
            _current?.Use();
            HideMenu();
        }

        private void HandleDrop()
        {
            Debug.Log($"[ItemActionMenu] HandleDrop called for item: {_current}");
            _current?.Drop();
            HideMenu();
        }

        private void HandleEquip()
        {
            Debug.Log($"[ItemActionMenu] HandleEquip called for item: {_current}");
            _current?.Equip();
            HideMenu();
        }

        private void HandleUnequip()
        {
            Debug.Log($"[ItemActionMenu] HandleUnequip called for item: {_current}");
            _current?.Unequip();
            HideMenu();
        }
    }
}
