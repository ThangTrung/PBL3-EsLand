using System.Collections.Generic;
using UnityEngine;

namespace Core.Contracts.Pathfinding
{
    public interface IPathfinder
    {
        /// <summary>
        /// Tìm đường từ điểm Start đến điểm Target.
        /// Trả về null hoặc list rỗng nếu không tìm được đường.
        /// Cung cấp entityRadius để kiểm tra kích thước thật của nhân vật, tránh kẹt.
        /// </summary>
        List<Vector3> FindPath(Vector3 startPosition, Vector3 targetPosition, float entityRadius = 0f);
    }
}
