namespace Script.Interfaces
{
    public interface IInteractable
    {
        string InteractionAnimationTrigger { get; }
        void Interact(Entities.Character interactor);
    }
}
