namespace Script.Entities.AI
{
    public interface IEnemyState
    {
        void Enter(Enemy enemy);
        void Execute(Enemy enemy);
        void Exit(Enemy enemy);
    }
}
