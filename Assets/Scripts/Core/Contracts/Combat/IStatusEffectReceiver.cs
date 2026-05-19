namespace Core.Contracts.Combat
{
    public interface IStatusEffectReceiver
    {
        void ApplyEffect(IStatusEffect effect);
    }
}
