using Data.Items;
namespace Core.Contracts.Inventory
{
    public interface IInventorySlot
    {
        ItemData ItemData { get; }
        int Amount { get; }
        int CurrentDurability { get; }
        bool IsEmpty { get; }
        float DurabilityPercent { get; }
        void AddAmount(int delta);
        void ReduceDurability(int amount);
        void RepairDurability(int amount);
    }
}

