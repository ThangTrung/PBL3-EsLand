using Core.Contracts.Inventory;
using Data.Equipment;

namespace Core.Contracts.Shared
{
    public interface IItemActionHandler
    {
        void UseItem(IInventorySlot slot);
        void DropItem(IInventorySlot slot);
        void EquipItem(IInventorySlot slot);
        void UnequipItem(EquipSlot slot);
        bool IsEquipped(IInventorySlot slot);
    }
}


