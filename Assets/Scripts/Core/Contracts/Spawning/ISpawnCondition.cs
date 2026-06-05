namespace Core.Contracts.Spawning
{
    /// <summary>
    /// Giao diện chuẩn mực cho các điều kiện sinh Boss/Quái.
    /// Áp dụng Strategy Pattern để mở rộng không giới hạn các loại điều kiện.
    /// </summary>
    public interface ISpawnCondition
    {
        bool IsMet(UnityEngine.Transform player);
        string GetFeedbackMessage();
    }
}