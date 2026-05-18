using Gameplay.AI.Enemies;

namespace Gameplay.AI.Bosses
{
    public interface IBossPhase
    {
        void EnterPhase(OgreBossEnemy boss);
        void ExecutePhase(OgreBossEnemy boss);
        void ExitPhase(OgreBossEnemy boss);
        
        /// <summary>
        /// The HP threshold to enter this phase (e.g., 0.66 for 66%).
        /// </summary>
        float HPThreshold { get; }
    }
}