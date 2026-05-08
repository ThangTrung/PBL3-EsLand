using Core.Types;

namespace Core.Contracts.Equipment
{
    public interface IGatheringTool
    {
        ToolType Type { get; }
        float GatherSpeedMultiplier { get; }
        int Tier { get; }
    }
}
