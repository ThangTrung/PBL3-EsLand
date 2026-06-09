namespace Core.Contracts.Environment
{
    public interface IArenaBarrier
    {
        void Lock();
        void Unlock();
    }
}