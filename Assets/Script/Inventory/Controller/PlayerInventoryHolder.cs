using Core.Contracts.Equipment;
using UnityEngine;
using Core.Contracts.Inventory;

namespace Gameplay.Inventory
{
    /// <summary>
    /// Component đại diện cho một thực thể có thể chứa Inventory (Player, NPC, Chest, ...).
    /// Chỉ làm nhiệm vụ holder, không xử lý logic inventory.
    /// </summary>
    public class PlayerInventoryHolder : MonoBehaviour, IInventoryHolder
    {
        [SerializeField] private InventoryController inventoryController;

        public PlayerInventoryHolder(IEquipmentController equipmentManager)
        {
            EquipmentManager = equipmentManager;
        }

        public IInventory Inventory => inventoryController;

        public IEquipmentController EquipmentManager { get; }
        //public IEquipmentController EquipmentManager => null; // Có thể mở rộng sau nếu có hệ thống Equipment

        private void Awake()
        {
            // Nếu chưa gán trên Inspector, tự tìm trên GameObject
            if (inventoryController == null)
                inventoryController = GetComponent<InventoryController>();
        }
    }
}

