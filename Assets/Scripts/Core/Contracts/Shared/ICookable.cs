namespace Core.Contracts.Shared
{
    /// <summary>
    /// Interface cho các vật phẩm có thể nấu/nung trong lò.
    /// </summary>
    public interface ICookable
    {
        bool IsCookable { get; }
        Data.Items.ItemData CookingResult { get; }
        float CookingTime { get; }
    }
}
