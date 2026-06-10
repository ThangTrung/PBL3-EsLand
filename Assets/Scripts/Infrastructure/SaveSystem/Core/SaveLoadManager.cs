using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Infrastructure.SaveSystem.Data;
using Infrastructure.SaveSystem.Core;

namespace Infrastructure.SaveSystem.Core
{
    /// <summary>
    /// Trái tim của hệ thống lưu trữ. Đã được chuyển đổi sang 100% Cloud Save (Duy nhất Server MASTER).
    /// </summary>
    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveLoadManager Instance { get; private set; }
        public bool IsLoading { get; private set; } = false;

        [Header("Cấu hình Cloud (Bật/Tắt tự động)")]
        [SerializeField] private bool useCloudSave = false; 

        [Header("Cấu hình Auto-Save")]
        [SerializeField] private bool useAutoSave = true; 
        [SerializeField] private float autoSaveInterval = 60f; 
        private float _autoSaveTimer = 0f;

        private GameData gameData;
        private List<ISaveable> saveableObjects;
        
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

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            // Không khởi tạo local save nữa. Gốc rễ dữ liệu là Cloud.
            
            // Chỉ tự động Load nếu KHÔNG ở màn hình khởi đầu (Login/MainMenu)
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName != "Login" && sceneName != "MainMenu" && sceneName != "Loading")
            {
                RefreshAndLoad();
            }
            
            _autoSaveTimer = 0f;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // Khi load sang map gameplay, tự động nạp dữ liệu từ bộ nhớ vào các ISaveable
            if (scene.name != "Login" && scene.name != "MainMenu" && scene.name != "Loading")
            {
                Debug.Log($"[SaveSystem] Scene '{scene.name}' loaded. Refreshing data...");
                RefreshAndLoad();
            }
        }

        private void RefreshAndLoad()
        {
            saveableObjects = FindAllSaveableObjects();
            if (gameData != null)
            {
                ProcessLoadedData();
            }
        }

        /// <summary>
        /// Kích hoạt và cấu hình Cloud Save tại Runtime (Gọi từ UI Login).
        /// </summary>
        public void EnableCloudMode(string userId, string serverIp, Action<bool, string> onComplete = null)
        {
            useCloudSave = true;
            _cloudDataHandler = new CloudDataHandler(serverIp, userId);
            
            // Lập tức thử tải dữ liệu từ mây về bộ nhớ tạm (gameData)
            LoadGameFromCloud(onComplete);
        }

        public bool HasData()
        {
            if (gameData == null) return false;
            
            // Logic đơn giản: Nếu có bất kỳ inventory nào có chứa item hoặc dữ liệu người chơi
            return gameData.inventories.Any(inv => inv.slots != null && inv.slots.Count > 0) || gameData.playerHealth < 100f;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K)) { SaveGame(); }
            if (Input.GetKeyDown(KeyCode.L)) { LoadGame(); }

            // Auto-Save: Chạy ngầm định kỳ (Chỉ lên Cloud)
            if (useAutoSave && useCloudSave && !IsLoading)
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
        /// Tải game từ Cloud.
        /// </summary>
        public void LoadGame()
        {
            if (!useCloudSave || _cloudDataHandler == null)
            {
                Debug.LogWarning("[SaveSystem] Cloud Save is not enabled. Cannot load.");
                return;
            }

            IsLoading = true;
            LoadGameFromCloud((success, msg) => {
                if (!success)
                {
                    Debug.LogError($"[SaveSystem] Cloud Load failed: {msg}");
                }
            });
        }

        private void LoadGameFromCloud(Action<bool, string> onComplete = null)
        {
            IsLoading = true;
            if (_cloudDataHandler == null)
            {
                onComplete?.Invoke(false, "Cloud handler not initialized.");
                return;
            }

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
            if (this.gameData == null) 
            {
                Debug.LogWarning("[SaveSystem] GameData is null, skipping process.");
                IsLoading = false;
                return;
            }

            if (saveableObjects == null) saveableObjects = FindAllSaveableObjects();

            Debug.Log($"[SaveSystem] Processing data for {saveableObjects.Count} saveable objects...");
            try 
            {
                foreach (ISaveable saveableObj in saveableObjects)
                {
                    if (saveableObj != null)
                    {
                        var mono = saveableObj as MonoBehaviour;
                        string objName = mono != null ? mono.gameObject.name : "Unknown";
                        Debug.Log($"[SaveSystem] Loading data for: {objName} ({saveableObj.GetType().Name})");
                        saveableObj.LoadData(gameData);
                    }
                }
                Debug.Log("[SaveSystem] Data processed successfully to all objects.");
            }
            catch (Exception e) 
            { 
                Debug.LogError($"[SaveSystem] Error processing data: {e.Message}\n{e.StackTrace}"); 
            }
            finally { IsLoading = false; }
        }

        public void SaveGame()
        {
            if (IsLoading || gameData == null || _isQuitting || !useCloudSave) return;

            GatherDataToSave();
            
            if (_cloudDataHandler != null)
            {
                Debug.Log("[SaveSystem] Saving game to cloud...");
                StartCoroutine(_cloudDataHandler.SaveRoutine(gameData, null)); 
            }
        }

        public void SyncToCloudManual(Action<bool, string> onComplete)
        {
            if (IsLoading || gameData == null)
            {
                onComplete?.Invoke(false, "System is busy or data is null.");
                return;
            }

            if (!useCloudSave || _cloudDataHandler == null)
            {
                onComplete?.Invoke(false, "Cloud Save is not enabled.");
                return;
            }

            GatherDataToSave();

            StartCoroutine(_cloudDataHandler.SaveRoutine(gameData, onComplete));
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

        /// <summary>
        /// Reset hệ thống về trạng thái ban đầu (Khi Logout).
        /// </summary>
        public void ResetSystem()
        {
            gameData = null;
            saveableObjects?.Clear();
            useCloudSave = false;
            _cloudDataHandler = null;
            Debug.Log("[SaveSystem] System reset completed.");
        }

        public void DeleteSaveData()
        {
            // Với Cloud Save, xóa data có nghĩa là Reset bộ nhớ và có thể gửi yêu cầu xóa lên Server (Tùy nhu cầu)
            useAutoSave = false; 
            gameData = null;
            Debug.LogWarning("[SaveSystem] Local GameData memory cleared.");
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
            if (useCloudSave)
            {
                // Lưu ý: Coroutine có thể không kịp chạy xong khi Quit trên Editor.
                // Một hệ thống Production sẽ dùng WebRequest đồng bộ hoặc chờ.
                SaveGame();
            }
        }
    }
}
