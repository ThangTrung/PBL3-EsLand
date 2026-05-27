namespace Core.Contracts.Shared
{
    /// <summary>
    /// Interface cho các đối tượng có khả năng phân lớp (tầng/layer).
    /// Giúp xác định thực thể đang ở tầng nào trong không gian 2D.
    /// </summary>
    public interface ILayerable
    {
        int CurrentLayer { get; }
        void SetLayer(int newLayer);
        
        // Sự kiện thông báo khi tầng thay đổi để cập nhật hiển thị hoặc vật lý
        System.Action<int> OnLayerChanged { get; set; }
    }
}
