using Infrastructure.SaveSystem.Data;

namespace Infrastructure.SaveSystem.Core
{
    public interface ISaveable
    {
        void SaveData(GameData data);
        void LoadData(GameData data);
    }
}
