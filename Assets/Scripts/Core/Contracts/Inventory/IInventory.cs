using System;
using System.Collections.Generic;
using Core.Contracts.Shared;
using Data.Items;
namespace Core.Contracts.Inventory
{
    /// <summary>
    /// Giao diện chuẩn cho hệ thống Túi đồ (Inventory).
    /// Quản lý việc thêm, gỡ và đếm vật phẩm.
    /// </summary>
    public interface IInventory
    {
        IReadOnlyList<IInventorySlot> Slots { get; }
        int Capacity { get; }
        int UsedSlots { get; }

        /// <summary> Sự kiện kích hoạt mỗi khi có sự thay đổi về vật phẩm hoặc số lượng </summary>
        event Action OnInventoryChanged;
        void NotifyChanged();

        /// <summary> Thêm vật phẩm vào túi đồ. Tự động tìm stack còn trống. </summary>
        /// <returns> True nếu thêm thành công </returns>
        bool AddItem(ItemData item, int amount = 1);

        /// <summary> Gỡ bỏ một lượng vật phẩm khỏi túi đồ. </summary>
        bool RemoveItem(ItemData item, int amount = 1);

        /// <summary> Tiêu thụ vật phẩm tại một Slot cụ thể (vd: khi nấu ăn hoặc dùng item) </summary>
        void ConsumeSlot(IInventorySlot slot, int amount = 1);

        /// <summary> Xóa bỏ toàn bộ nội dung của một Slot </summary>
        bool RemoveSlot(IInventorySlot slot);

        /// <summary> Đếm tổng số lượng của một loại vật phẩm trong toàn bộ túi đồ </summary>
        int CountItem(ItemData item);

        /// <summary> Xóa sạch túi đồ </summary>
        void Clear();

        /// <summary> Đối tượng xử lý các hành động logic với item (Dùng, Vứt, Trang bị) </summary>
        IItemActionHandler ActionHandler { get; }

        /// <summary> Hoán đổi vị trí giữa hai Slot (Sắp xếp túi đồ) </summary>
        void SwapSlots(int indexA, int indexB);
    }
}

