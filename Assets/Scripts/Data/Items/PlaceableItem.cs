using Core.Contracts.Shared;
using Data.Building;
using Gameplay.Characters;
using UnityEngine;
using Gameplay.Building;

namespace Data.Items
{
    /// <summary>
    /// Vật phẩm mang tính chất "Bản vẽ" hoặc "Mô hình thu nhỏ" của công trình.
    /// Có thể cất trong túi đồ như mọi vật phẩm khác.
    /// </summary>
    [CreateAssetMenu(fileName = "New Placeable Item", menuName = "Inventory/ItemData/Placeable")]
    public class PlaceableItem : ItemData, IItemUsable
    {
        [Header("Building Reference")]
        [Tooltip("Dữ liệu của công trình thật sẽ được sinh ra khi người chơi đặt xuống đất")]
        [SerializeField] private BuildingData targetBuilding;

        public BuildingData TargetBuilding => targetBuilding;

        /// <summary>
        /// Logic được gọi khi người chơi bấm nút "Sử dụng" món đồ này trong túi.
        /// </summary>
        public bool Use(Character user)
        {
            if (user == null || targetBuilding == null) return false;

            // Chuyển hệ thống sang chế độ "Cầm công trình chuẩn bị đặt" (Placement Mode).
            if (BuildingPlacementManager.Instance != null)
            {
                BuildingPlacementManager.Instance.StartPlacement(this, user);
            }
            else
            {
                Debug.LogError("Chưa có BuildingPlacementManager trong Scene!");
            }

            // Return False để Bảng Inventory KHÔNG tự động trừ vật phẩm trong túi ngay lập tức.
            // Vật phẩm chỉ bị trừ khi người chơi thực sự click chuột đặt công trình xuống đất thành công (Xử lý trong BuildingPlacementManager).
            return false;
        }
    }
}
