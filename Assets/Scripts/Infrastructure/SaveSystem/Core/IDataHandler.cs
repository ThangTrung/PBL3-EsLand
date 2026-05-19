using Infrastructure.SaveSystem.Data;

namespace Infrastructure.SaveSystem.Core
{
    public interface IDataHandler
    {
        GameData Load();
        void Save(GameData data);
    }
}
