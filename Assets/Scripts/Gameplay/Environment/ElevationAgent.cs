using UnityEngine;
using Layer;

namespace Gameplay.Environment
{
    /// <summary>
    /// Chịu trách nhiệm tiếp nhận yêu cầu thay đổi tầng cao độ (Elevation)
    /// và cập nhật Sorting Layer cho nhân vật.
    /// </summary>
    [RequireComponent(typeof(AutoAssignSortingLayer))]
    public class ElevationAgent : MonoBehaviour
    {
        private AutoAssignSortingLayer _layerAssigner;
        
        [Header("Trạng thái hiện tại")]
        [SerializeField, ReadOnly] private string currentElevation;
        public string CurrentElevation => currentElevation;

        private void Awake()
        {
            _layerAssigner = GetComponent<AutoAssignSortingLayer>();
            currentElevation = _layerAssigner.targetSortingLayer;
        }

        /// <summary>
        /// Được gọi bởi ElevationGateway khi nhân vật chạm vào.
        /// </summary>
        /// <param name="newElevationLayer">Tên Sorting Layer mới (VD: Elevation_B)</param>
        public void ChangeElevation(string newElevationLayer)
        {
            if (currentElevation == newElevationLayer) return;

            currentElevation = newElevationLayer;
            _layerAssigner.targetSortingLayer = newElevationLayer;
            
            // Cập nhật ngay lập tức hình ảnh của toàn bộ object con
            _layerAssigner.ApplyLayer();

            Debug.Log($"<color=cyan>[Elevation]</color> {gameObject.name} đã chuyển sang tầng: <b>{newElevationLayer}</b>");
        }
    }

    // Attribute đơn giản để hiển thị trong Inspector mà không cho sửa (Tùy chọn)
    public class ReadOnlyAttribute : PropertyAttribute { }
}