using Core.Contracts.Shared;
using UnityEngine;

namespace Core.Contracts.Services
{
    /// <summary>
    /// Service quản lý logic chuyển đổi giữa các layer.
    /// Kiểm tra khả năng tương tác giữa các thực thể dựa trên tầng hiện tại.
    /// </summary>
    public interface ILayerService
    {
        /// <summary>
        /// Kiểm tra xem hai thực thể có cùng tầng để tương tác hay không.
        /// </summary>
        bool CanInteract(ILayerable requester, ILayerable target);

        /// <summary>
        /// Thực hiện chuyển tầng cho một thực thể.
        /// </summary>
        void ChangeLayer(ILayerable entity, int targetLayer);
        
        /// <summary>
        /// Lấy LayerMask tương ứng với tầng hiện tại (nếu dùng Physics2D).
        /// </summary>
        int GetPhysicsLayerMask(int logicalLayer);
    }
}
