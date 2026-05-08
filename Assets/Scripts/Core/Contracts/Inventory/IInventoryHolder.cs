using Core.Contracts.Equipment;

namespace Core.Contracts.Inventory
{
    public interface IInventoryHolder
    {
        IInventory Inventory { get; }
        IEquipmentController EquipmentManager { get; }
    }
}


