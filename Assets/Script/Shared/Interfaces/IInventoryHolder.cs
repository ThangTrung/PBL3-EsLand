using Script.Equipment.Core;
using Script.Equipment.Interfaces;

namespace Script.Shared.Interfaces
{
    public interface IInventoryHolder
    {
        Script.Inventory.Interfaces.IInventory Inventory { get; }
        IEquipmentManager EquipmentManager { get; }
    }
}