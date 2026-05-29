using UnityEngine;

namespace Core.Contracts.Shared
{
    /// <summary>
    /// Interface định nghĩa khả năng hồi sinh của một đối tượng.
    /// Cho phép thiết lập điểm hồi sinh và thực hiện logic hồi sinh.
    /// </summary>
    public interface IRespawnable
    {
        /// <summary>
        /// Vị trí hồi sinh hiện tại.
        /// </summary>
        Vector3 RespawnPoint { get; set; }

        /// <summary>
        /// Thiết lập điểm hồi sinh mới.
        /// </summary>
        /// <param name="position">Tọa độ thế giới của điểm hồi sinh.</param>
        void SetRespawnPoint(Vector3 position);

        /// <summary>
        /// Thực hiện quy trình hồi sinh.
        /// </summary>
        void Respawn();
    }
}
