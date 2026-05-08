namespace Script.Shared.Interfaces
{
    public interface IStatModifierProvider
    {
        float GetDamageModifier();
        float GetDefenseModifier();
        float GetSpeedModifier();
        float GetHealthModifier();
    }
}
