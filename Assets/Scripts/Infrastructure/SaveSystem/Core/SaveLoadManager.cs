using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Infrastructure.SaveSystem.Data;
using Infrastructure.SaveSystem.Core;

namespace Infrastructure.SaveSystem.Core
{
    /// <summary>
    /// Trái tim của hệ thống lưu trữ. Đã được nâng cấp để hỗ trợ "Hybrid Save" (Lưu Local + Đồng bộ Cloud).
    /// </summary>
    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }
        public bool IsLoading { get; private set; } = false;

        [Header("Cấu hình Lưu Trữ Cục Bộ (Local)")]
        [SerializeField] private string fileName = "pbl3_esland_save.json";
        
        [Header("Cấu hình Cloud (Bật/Tắt tự động)")]
        [SerializeField] private bool useCloudSave = false; // Bật nếu muốn lúc nào cũng lưu mây (phải thiết lập IP/ID trước)

        [Header("Cấu hình Auto-Save")]
        [SerializeField] private bool useAutoSave = true; 
        [SerializeField] private float autoSaveInterval = 60f; 
        private float _autoSaveTimer = 0f;

        private GameData gameData;
        private List<ISaveable> saveableObjects;
        
        // Hỗ trợ Hybrid Save: Luôn có Local, Cloud có thể bật/tắt
        private IDataHandler _localDataHandler;
        private CloudDataHandler _cloudDataHandler;

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
            // 1. Luôn khởi tạo Local Save làm gốc rễ an toàn
            string projectRootPath = Application.dataPath + "/..";
            _localDataHandler = new FileDataHandler(projectRootPath, fileName);

            // 2. Khởi tạo Cloud nếu được bật mặc định (Demo nhanh)
            if (useCloudSave)
            {
                _cloudDataHandler = new CloudDataHandler("localhost", SystemInfo.deviceUniqueIdentifier);
            }

            saveableObjects = FindAllSaveableObjects();
            
            // 3. Tự động tải dữ liệu khi bắt đầu (Ưu tiên Cloud nếu có, fallback về Local)
            LoadGame();
            
            _autoSaveTimer = 0f;
        }

        /// <summary>
        /// Kích hoạt và cấu hình Cloud Save tại Runtime (Gọi từ UI Login).
        /// </summary>
        public void EnableCloudMode(string userId, string serverIp, Action<bool, string> onComplete = null)
        {
            useCloudSave = true;
            _cloudDataHandler = new CloudDataHandler(serverIp, userId);
            
            // Lập tức thử tải dữ liệu từ mây về
            LoadGameFromCloud(onComplete);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K)) { SaveGame(); }
            if (Input.GetKeyDown(KeyCode.L)) { LoadGame(); }

            // Auto-Save: Chạy ngầm định kỳ
            if (useAutoSave && !IsLoading)
            {
                _autoSaveTimer += Time.deltaTime;
                if (_autoSaveTimer >= autoSaveInterval)
                {
                    SaveGame();
                    _autoSaveTimer = 0f;
                }
            }
        }

        public void NewGame()
        {
            gameData = new GameData();
        }

        /// <summary>
        /// Tải game. Ưu tiên lấy từ Cloud trước, nếu lỗi hoặc không bật Cloud thì lấy từ Local.
        /// </summary>
        public void LoadGame()
        {
            IsLoading = true;
            
            if (useCloudSave && _cloudDataHandler != null)
            {
                LoadGameFromCloud((success, msg) => {
                    if (!success)
                    {
                        Debug.LogWarning($"[SaveSystem] Cloud Load failed ({msg}). Fallback to Local Load.");
                        LoadGameFromLocal();
                    }
                });
            }
            else
            {
                LoadGameFromLocal();
            }
        }

        private void LoadGameFromLocal()
        {
            this.gameData = _localDataHandler.Load();
            ProcessLoadedData();
        }

        private void LoadGameFromCloud(Action<bool, string> onComplete = null)
        {
            IsLoading = true;
            StartCoroutine(_cloudDataHandler.LoadRoutine((loadedData, message) => {
                if (loadedData != null)
                {
                    this.gameData = loadedData;
                    ProcessLoadedData();
                    onComplete?.Invoke(true, message);
                }
                else
                {
                    IsLoading = false;
                    onComplete?.Invoke(false, message);
                }
            }));
        }

        private void ProcessLoadedData()
        {
            if (this.gameData == null) NewGame();

            try 
            {
                foreach (ISaveable saveableObj in saveableObjects)
                {
                    saveableObj.LoadData(gameData);
                }
            }
            catch (Exception e) { Debug.LogError($"[SaveSystem] Error processing data: {e.Message}"); }
            finally { IsLoading = false; }
        }

        /// <summary>
        /// Lưu game tự động. Ghi vào Local và đẩy lên Cloud (nếu bật).
        /// </summary>
        public void SaveGame()
        {
            if (IsLoading || _localDataHandler == null || gameData == null || _isQuitting) return;

            GatherDataToSave();
            
            // 1. Luôn lưu vào ổ cứng (Backup an toàn)
            _localDataHandler.Save(gameData);

            // 2. Bắn lên Cloud (Chạy ngầm không ảnh hưởng game)
            if (useCloudSave && _cloudDataHandler != null)
            {
                StartCoroutine(_cloudDataHandler.SaveRoutine(gameData, null)); // Null callback cho auto-save
            }
        }

        /// <summary>
        /// API cho UI gọi khi người chơi bấm nút "Đồng bộ Cloud".
        /// Có callback để UI biết hiện thông báo Thành công / Thất bại.
        /// </summary>
        public void SyncToCloudManual(Action<bool, string> onComplete)
        {
            if (IsLoading || gameData == null)
            {
                onComplete?.Invoke(false, "System is busy or data is null.");
                return;
            }

            GatherDataToSave();
            
            // Vẫn lưu Local để chắc chắn
            _localDataHandler.Save(gameData);

            // Gửi lên mây và chờ phản hồi
            if (useCloudSave && _cloudDataHandler != null)
            {
                StartCoroutine(_cloudDataHandler.SaveRoutine(gameData, onComplete));
            }
            else
            {
                onComplete?.Invoke(false, "Cloud Save is not enabled or configured.");
            }
        }

        private void GatherDataToSave()
        {
            if (saveableObjects == null) saveableObjects = FindAllSaveableObjects();
            saveableObjects.RemoveAll(s => s == null || (s is UnityEngine.Object obj && obj == null));

            foreach (ISaveable saveableObj in saveableObjects)
            {
                saveableObj.SaveData(gameData);
            }
        }

        public void DeleteSaveData()
        {
            useAutoSave = false; 
            if (_localDataHandler != null) _localDataHandler.Delete();
            gameData = null;
        }

        private List<ISaveable> FindAllSaveableObjects()
        {
            var saveables = new List<ISaveable>();
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>(true);
            if (allMonoBehaviours == null) return saveables;

            foreach (var mono in allMonoBehaviours)
            {
                if (mono is ISaveable saveable) saveables.Add(saveable);
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
        
        private void OnApplicationQuit()
        {
            _isQuitting = true;
            SaveGame();
        }
    }
}
