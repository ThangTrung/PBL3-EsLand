namespace Script.Equipment.Interfaces
{
    public interface IGatheringTool
    {
        ToolType Type { get; }
        float GatherSpeedMultiplier { get; }
        int Tier { get; }
    }
}