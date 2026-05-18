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

        [Header("Cấu hình Auto-Save (SOLID)")]
        [SerializeField] private bool useAutoSave = true; // Bật/tắt tính năng lưu tự động
        [SerializeField] private float autoSaveInterval = 60f; // Thời gian giãn cách giữa các lần lưu (giây)
        private float _autoSaveTimer = 0f;

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
            if (useCloudSave)
            {
                dataHandler = new CloudDataHandler();
            }
            else
            {
                string projectRootPath = Application.dataPath + "/..";
                dataHandler = new FileDataHandler(projectRootPath, fileName);
            }

            saveableObjects = FindAllSaveableObjects();
            LoadGame();
            
            // Khởi tạo lại bộ đếm khi game bắt đầu
            _autoSaveTimer = 0f;
        }

        private void Update()
        {
            // Luồng 1: Nhận lệnh thao tác thủ công từ lập trình viên/người chơi
            if (Input.GetKeyDown(KeyCode.S)) { SaveGame(); }
            if (Input.GetKeyDown(KeyCode.L)) { LoadGame(); }

            // Luồng 2: Hệ thống Auto-Save chạy ngầm (Đảm bảo SRP)
            if (useAutoSave && !IsLoading)
            {
                _autoSaveTimer += Time.deltaTime;
                if (_autoSaveTimer >= autoSaveInterval)
                {
                    SaveGame();
                    _autoSaveTimer = 0f; // Reset lại đồng hồ sau khi lưu thành công
                    Debug.Log("<color=yellow>[SaveLoadManager] HỆ THỐNG: Đã kích hoạt Auto-Save định kỳ!</color>");
                }
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
                Debug.Log("[SaveLoadManager] Không có dữ liệu save. Khởi tạo game mới.");
                NewGame();
            }

            try 
            {
                foreach (ISaveable saveableObj in saveableObjects)
                {
                    saveableObj.LoadData(gameData);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("<color=red>[SaveLoadManager] Có lỗi khi LoadData ở một object:</color> " + e.Message);
            }
            finally 
            {
                IsLoading = false; 
            }
        }

        public void SaveGame()
        {
            if (IsLoading) return;

            saveableObjects.RemoveAll(s => s is Object obj && obj == null);

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
            IEnumerable<ISaveable> saveables = FindObjectsOfType<MonoBehaviour>(true).OfType<ISaveable>();
            return new List<ISaveable>(saveables);
        }
        
        public void RegisterDestroyedEntity(string id)
        {
            if (gameData != null && !gameData.destroyedEntityIDs.Contains(id))
            {
                gameData.destroyedEntityIDs.Add(id);
            }
        }
    }
}