using Core.Contracts.Shared;
using UI.Equipment;
using UI.Inventory;
using UI.ItemActions;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public InventoryPanelUI inventoryUI;
    public EquipmentPanelUI equipmentUI; // Tương tự làm y chang cho Equipment
    public ItemActionMenu actionMenu;

    private void Start()
    {
        // 1. Nhạc trưởng lắng nghe tiếng hét từ túi đồ
        inventoryUI.OnActionMenuRequested += OpenActionMenu;
        inventoryUI.OnInventoryClosed += actionMenu.HideMenu;

        // 2. Nhạc trưởng lắng nghe tiếng hét từ trang bị (Sau này mày viết thêm)
        // equipmentUI.OnActionMenuRequested += OpenActionMenu;
        // equipmentUI.OnEquipmentClosed += actionMenu.HideMenu;
    }

    private void OpenActionMenu(IActionableItem context, Vector3 pos)
    {
        actionMenu.ShowMenu(context, pos);
    }
}