using System;
using System.Collections.Generic;
using System.Linq;
using Core.Contracts.Equipment;
using Core.Contracts.Shared;
using Data.Equipment;
using Gameplay.Characters;
using UnityEngine;
using Infrastructure.SaveSystem.Core; // Để xài ISaveable
using Infrastructure.SaveSystem.Data; // Để xài GameData
using Data.Items; // Bổ sung để load ItemData

namespace Gameplay.Equipment
{
    // 🔥 ĐÃ THÊM: Kế thừa ISaveable để hệ thống tự quét thấy nó khi bấm Save
    public class EquipmentManager : MonoBehaviour, IEquipmentController, ISaveable
    {
        private readonly Dictionary<EquipSlot, IEquippable> _equippedItems = new Dictionary<EquipSlot, IEquippable>();
        private Character _character;

        public event Action<EquipSlot, IEquippable> OnItemEquipped;
        public event Action<EquipSlot, IEquippable> OnItemUnequipped;

        public IReadOnlyDictionary<EquipSlot, IEquippable> EquippedItems => _equippedItems;

        public void Initialize(Character character) => _character = character;

        public void Equip(IEquippable item)
        {
            if (item == null) return;
            if (_equippedItems.ContainsKey(item.Slot)) Unequip(item.Slot);
            
            _equippedItems[item.Slot] = item;
            item.OnEquip(_character);
            OnItemEquipped?.Invoke(item.Slot, item);
        }

        public void Unequip(EquipSlot slot)
        {
            if (!_equippedItems.TryGetValue(slot, out var item)) return;
            
            item.OnUnequip(_character);
            _equippedItems.Remove(slot);
            OnItemUnequipped?.Invoke(slot, item);
        }

        public float GetTotalDamageModifier() => _equippedItems.Values.OfType<IStatModifierProvider>().Sum(p => p.GetDamageModifier());
        public float GetTotalDefenseModifier() => _equippedItems.Values.OfType<IStatModifierProvider>().Sum(p => p.GetDefenseModifier());
        public float GetTotalSpeedModifier() => _equippedItems.Values.OfType<IStatModifierProvider>().Sum(p => p.GetSpeedModifier());
        public float GetTotalHealthModifier() => _equippedItems.Values.OfType<IStatModifierProvider>().Sum(p => p.GetHealthModifier());

        public IEquippable GetEquippedItem(EquipSlot slot)
        {
            _equippedItems.TryGetValue(slot, out var item);
            return item;
        }

        // ==========================================
        // 🔥 PHẦN CODE LƯU & LOAD ĐƯỢC THÊM MỚI
        // ==========================================
        
        public void LoadData(GameData data)
        {
            // 1. Tháo hết đồ cũ (nếu có) để chuẩn bị reset
            var slotsToUnequip = _equippedItems.Keys.ToList();
            foreach (var slot in slotsToUnequip)
            {
                Unequip(slot);
            }

            // 2. Quét sạch thư mục Resources/Items để tìm file cấu hình của món đồ
            ItemData[] allItemsInResources = Resources.LoadAll<ItemData>("Items");

            // 3. Đắp đồ từ file save vào lại cho nhân vật
            foreach (var equipData in data.equippedItems)
            {
                ItemData foundItem = allItemsInResources.FirstOrDefault(i => i.name == equipData.itemID);
                
                if (foundItem != null && foundItem is IEquippable equippableItem)
                {
                    Equip(equippableItem);
                }
            }
        }

        public void SaveData(GameData data)
        {
            // Reset lại danh sách trên file JSON cho sạch
            data.equippedItems.Clear();

            // Vét toàn bộ đồ đang mặc nhét vào sổ ghi chép
            foreach (var kvp in _equippedItems)
            {
                if (kvp.Value is UnityEngine.Object objItem)
                {
                    data.equippedItems.Add(new EquippedItemSaveData
                    {
                        slot = kvp.Key,
                        itemID = objItem.name // Ghi đúng cái tên file (ví dụ: "Axe", "Defense")
                    });
                }
            }
        }
    }
}