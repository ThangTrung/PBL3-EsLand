using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Infrastructure.SaveSystem.Data;
using Infrastructure.SaveSystem.Core;

namespace Infrastructure.SaveSystem.Core
{
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
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Inject Handler
            if (useCloudSave)
            {
                dataHandler = new CloudDataHandler();
            }
            else
            {
                // Lưu ở thư mục gốc của Project để bạn dễ kiểm tra file JSON
                string projectRootPath = Application.dataPath + "/..";
                dataHandler = new FileDataHandler(projectRootPath, fileName);
            }

            saveableObjects = FindAllSaveableObjects();
            LoadGame();
        }

        private void Update()
        {
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
            
            // Logic Cloud Save (Coroutine) hoặc Local Save (Synchronous)
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
                Debug.Log("[SaveLoadManager] Không có dữ liệu save. Khởi tạo game mới.");
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
            yield return new WaitForSeconds(0.2f);
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
                    if(success) Debug.Log("<color=cyan>[SaveLoadManager] CLOUD SAVE HOÀN TẤT!</color>");
                }));
            }
            else
            {
                dataHandler.Save(gameData);
                Debug.Log("<color=green>[SaveLoadManager] LOCAL SAVE HOÀN TẤT!</color>");
            }
        }

        private List<ISaveable> FindAllSaveableObjects()
        {
            // Tìm tất cả các component implement ISaveable trong Scene
            IEnumerable<ISaveable> saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>();
            return new List<ISaveable>(saveables);
        }
    }
}
