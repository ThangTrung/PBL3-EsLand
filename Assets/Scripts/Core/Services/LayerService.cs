using Core.Contracts.Services;
using Core.Contracts.Shared;
using UnityEngine;

namespace Core.Services
{
    /// <summary>
    /// Triển khai thực tế của ILayerService.
    /// Quản lý logic kiểm tra và chuyển đổi tầng cho thực thể.
    /// </summary>
    public class LayerService : ILayerService
    {
        public bool CanInteract(ILayerable requester, ILayerable target)
        {
            if (requester == null || target == null) return false;
            
            // Chỉ cho phép tương tác nếu ở cùng một tầng (Layer)
            return requester.CurrentLayer == target.CurrentLayer;
        }

        public void ChangeLayer(ILayerable entity, int targetLayer)
        {
            if (entity == null) return;
            
            // Thực hiện chuyển tầng trong logic
            entity.SetLayer(targetLayer);
            
            Debug.Log($"Entity {entity} đã chuyển sang Layer: {targetLayer}");
        }

        public int GetPhysicsLayerMask(int logicalLayer)
        {
            // Tùy biến logic: Ví dụ Logical Layer 0 -> Physics Layer "Ground"
            // Logical Layer 1 -> Physics Layer "UpperFloor"
            // Ở đây trả về mask mặc định hoặc logic cụ thể của dự án
            return LayerMask.GetMask("Default", "Interactable"); 
        }
    }
}
