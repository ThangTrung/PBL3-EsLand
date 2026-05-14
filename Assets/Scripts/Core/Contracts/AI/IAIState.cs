using Gameplay.AI;

namespace Core.Contracts.AI
{
    public interface IAIState
    {
        void Enter(EnemyBase enemy);
        void Execute(EnemyBase enemy);
        void Exit(EnemyBase enemy);
    }
}
