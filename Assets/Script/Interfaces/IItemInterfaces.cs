namespace Script.Interfaces
{
    public interface IItemUsable
    {
        bool Use(Entities.Character user);
    }

    public interface IEquippable
    {
        Items.EquipSlot EquipSlot { get; }
        void OnEquip(Entities.Character character);
        void OnUnequip(Entities.Character character);
    }

    public interface IDurable
    {
        int MaxDurability { get; }
    }

    public interface IWeapon
    {
        float Damage { get; }
    }

    public interface IGatheringTool
    {
        Items.ToolType ToolType { get; }
        float GatherSpeedMultiplier { get; }
        int Tier { get; }
    }

    public interface IStatModifierProvider
    {
        float GetDamageModifier();
        float GetDefenseModifier();
        float GetSpeedModifier();
        float GetHealthModifier();
    }

    public interface IInventorySlot
    {
        Items.Item Item { get; }
        int Amount { get; }
        int CurrentDurability { get; }
        bool IsEmpty { get; }
        float DurabilityPercent { get; }
        void AddAmount(int delta);
        void ReduceDurability(int amount);
        void RepairDurability(int amount);
    }

    public interface IItemActionHandler
    {
        void UseItem(IInventorySlot slot);
        void DropItem(IInventorySlot slot);
        void EquipItem(IInventorySlot slot);
        void UnequipItem(Items.EquipSlot slot);
        bool IsEquipped(IInventorySlot slot);
    }

    public interface IInventory
    {
        System.Collections.Generic.IReadOnlyList<IInventorySlot> Slots { get; }
        int Capacity { get; }
        int UsedSlots { get; }
        event System.Action OnInventoryChanged;
        void NotifyChanged(); // Added this
        bool AddItem(Items.Item item, int amount = 1);
        void ConsumeSlot(IInventorySlot slot, int amount = 1);
        bool RemoveSlot(IInventorySlot slot);
        int CountItem(Items.Item item);
        void Clear();
        IItemActionHandler ActionHandler { get; }
        void SwapSlots(int indexA, int indexB);
    }

    public interface IInventoryHolder
    {
        IInventory Inventory { get; }
    }

    public interface IInventorySlotUI
    {
        int SlotIndex { get; }
        void Refresh(IInventorySlot slotData);
        void SetHighlight(bool active);
    }

    public interface IInventoryUI
    {
        bool IsVisible { get; }
        void SetVisible(bool visible);
        void RefreshUI();
        void SelectSlot(int index);
    }
}
