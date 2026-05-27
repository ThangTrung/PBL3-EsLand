using Gameplay.Characters;

namespace Core.Contracts.Shared
{
    public interface IInteractable
    {
        bool CanInteract(Character interactor);
        float GetStaminaCost(Character interactor);
        void Interact(Character interactor);
    }
}

