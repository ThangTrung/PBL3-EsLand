using System.Collections.Generic;

public interface IDataHandler
{
    GameData Load();
    void Save(GameData data);
}
