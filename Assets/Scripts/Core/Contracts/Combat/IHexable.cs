namespace Core.Contracts.Combat
{
    public interface IHexable
    {
        /// <summary>
        /// Called when the entity is hit by a Hex/Transformation spell.
        /// </summary>
        void OnHexed();
    }
}