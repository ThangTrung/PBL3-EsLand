using System.Collections;

namespace Core.Contracts.Shared
{
    /// <summary>
    /// Interface định nghĩa một dịch vụ chuyển cảnh (ví dụ: Fade đen, Mây trôi, v.v.)
    /// Giúp tách biệt logic Gameplay và hiệu ứng hình ảnh.
    /// </summary>
    public interface ITransitionService
    {
        /// <summary>
        /// Bắt đầu hiệu ứng che phủ màn hình.
        /// </summary>
        IEnumerator FadeOut(float duration, string message = "");

        /// <summary>
        /// Bắt đầu hiệu ứng làm hiện lại màn hình.
        /// </summary>
        IEnumerator FadeIn(float duration);

        /// <summary>
        /// Kiểm tra xem có đang trong quá trình chuyển cảnh không.
        /// </summary>
        bool IsTransitioning { get; }
    }
}
