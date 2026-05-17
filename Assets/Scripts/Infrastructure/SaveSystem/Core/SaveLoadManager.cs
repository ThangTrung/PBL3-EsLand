using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }
    public bool IsLoading { get; private set; } = false;

    [Header("Cấu hình Lưu Trữ")]
    [SerializeField] private string fileName = "pbl3_esland_save.json";
    [SerializeField] private bool useCloudSave = false;

    private GameData gameData;
    private List<ISaveable> saveableObjects;
    private IDataHandler dataHandler;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        // Strategy Pattern Injection dựa trên biến useCloudSave
        if (useCloudSave)
        {
            dataHandler = new CloudDataHandler();
        }
        else
        {
            string projectRootPath = Application.dataPath + "/..";
            dataHandler = new FileDataHandler(projectRootPath, fileName);
            // dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        }

        saveableObjects = FindAllSaveableObjects();
        LoadGame();
    }

    private void Update()
    {
        // Fix bug spam request: Chỉ lưu/load khi nhấn phím
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveGame();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadGame();
        }
    }

    public void NewGame()
    {
        gameData = new GameData();
    }

    public void LoadGame()
    {
        IsLoading = true;
        if (useCloudSave && dataHandler is CloudDataHandler cloudHandler)
        {
            StartCoroutine(cloudHandler.LoadRoutine((loadedData) => {
                this.gameData = loadedData;
                ProcessLoadedData();
            }));
        }
        else
        {
            this.gameData = dataHandler.Load();
            ProcessLoadedData();
        }
    }

    private void ProcessLoadedData()
    {
        if (this.gameData == null)
        {
            Debug.Log("Không có dữ liệu save. Khởi tạo game mới.");
            NewGame();
        }

        foreach (ISaveable saveableObj in saveableObjects)
        {
            saveableObj.LoadData(gameData);
        }
        StartCoroutine(UnlockSaveRoutine());
    }

    private System.Collections.IEnumerator UnlockSaveRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        IsLoading = false;
    }

    public void SaveGame()
    {
        if (IsLoading) return;
        foreach (ISaveable saveableObj in saveableObjects)
        {
            saveableObj.SaveData(gameData);
        }

        if (useCloudSave && dataHandler is CloudDataHandler cloudHandler)
        {
            StartCoroutine(cloudHandler.SaveRoutine(gameData, (success) => {
                if(success) Debug.Log("<color=cyan>CLOUD SAVE HOÀN TẤT!</color>");
            }));
        }
        else
        {
            dataHandler.Save(gameData);
            Debug.Log("<color=green>LOCAL SAVE HOÀN TẤT!</color>");
        }
    }

    private List<ISaveable> FindAllSaveableObjects()
    {
        IEnumerable<ISaveable> saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>();
        return new List<ISaveable>(saveables);
    }
}
