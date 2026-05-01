namespace Core.Contracts.Shared
{
    public interface IStatModifierProvider
    {
        float GetDamageModifier();
        float GetDefenseModifier();
        float GetSpeedModifier();
        float GetHealthModifier();
    }
}

