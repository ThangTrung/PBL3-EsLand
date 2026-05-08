using Gameplay.Characters;

namespace Core.Contracts.Shared
{
    public interface IItemUsable
    {
        bool Use(Character user);
    }
}


