namespace Core.Contracts.Shared
{
    /// <summary>
    /// Interface for components that need to listen to UI state changes.
    /// Helps decouple UI from specific character classes.
    /// </summary>
    public interface IUIEventListener
    {
        void OnUIStateChanged(string uiName, bool isOpen);
    }
}
