using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    [Header("Cấu hình File")]
    [SerializeField] private string fileName = "pbl3_esland_save.json";

    private GameData gameData;
    private List<ISaveable> saveableObjects;
    private FileDataHandler dataHandler;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        saveableObjects = FindAllSaveableObjects();

        LoadGame();
    }

    public void NewGame()
    {
        gameData = new GameData();
    }

    public void LoadGame()
    {
        gameData = dataHandler.Load();

        if (this.gameData == null)
        {
            Debug.Log("Không có file save. Bắt đầu game mới.");
            NewGame();
        }

        foreach (ISaveable saveableObj in saveableObjects)
        {
            saveableObj.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        foreach (ISaveable saveableObj in saveableObjects)
        {
            saveableObj.SaveData(gameData);
        }

        dataHandler.Save(gameData);
        Debug.Log("<color=green>ĐÃ LƯU TOÀN BỘ GAME!</color>");
    }

    // Hàm tự động quét tìm tất cả ISaveable trong Scene
    private List<ISaveable> FindAllSaveableObjects()
    {
        IEnumerable<ISaveable> saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>();
        return new List<ISaveable>(saveables);
    }
}