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

        private bool _isQuitting = false;

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
            if (Input.GetKeyDown(KeyCode.K)) { SaveGame(); }
            if (Input.GetKeyDown(KeyCode.L)) { LoadGame(); }

            // Luồng 2: Hệ thống Auto-Save chạy ngầm (Đảm bảo SRP)
            if (useAutoSave && !IsLoading)
            {
                _autoSaveTimer += Time.deltaTime;
                if (_autoSaveTimer >= autoSaveInterval)
                {
                    SaveGame();
                    _autoSaveTimer = 0f; // Reset lại đồng hồ sau khi lưu thành công
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
                NewGame();
            }

            try 
            {
                foreach (ISaveable saveableObj in saveableObjects)
                {
                    saveableObj.LoadData(gameData);
                }
            }
            catch (System.Exception)
            {
            }
            finally 
            {
                IsLoading = false; 
            }
        }

        public void SaveGame()
        {
            if (IsLoading || dataHandler == null || gameData == null || _isQuitting) return;

            if (saveableObjects == null) saveableObjects = FindAllSaveableObjects();
            
            // Lọc bỏ các đối tượng đã bị hủy
            saveableObjects.RemoveAll(s => s == null || (s is Object obj && obj == null));

            foreach (ISaveable saveableObj in saveableObjects)
            {
                saveableObj.SaveData(gameData);
            }

            if (useCloudSave && dataHandler is CloudDataHandler cloudHandler)
            {
                StartCoroutine(cloudHandler.SaveRoutine(gameData, (success) => {
                }));
            }
            else
            {
                dataHandler.Save(gameData);
            }
        }

        public void DeleteSaveData()
        {
            useAutoSave = false; // Ngăn chặn auto-save sau khi xóa file
            if (dataHandler != null)
            {
                dataHandler.Delete();
            }
            gameData = null;
        }

        private List<ISaveable> FindAllSaveableObjects()
        {
            var saveables = new List<ISaveable>();
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>(true);
            
            if (allMonoBehaviours == null) return saveables;

            foreach (var mono in allMonoBehaviours)
            {
                if (mono is ISaveable saveable)
                {
                    saveables.Add(saveable);
                }
            }
            return saveables;
        }
        
        public void RegisterDestroyedEntity(string id)
        {
            if (gameData != null && !gameData.destroyedEntityIDs.Contains(id))
            {
                gameData.destroyedEntityIDs.Add(id);
            }
        }
        
        // Hàm của Unity TỰ ĐỘNG chạy khi người chơi thoát game hoặc Dev bấm nút Stop
        private void OnApplicationQuit()
        {
            _isQuitting = true;
            SaveGame();
        }
    }
}

