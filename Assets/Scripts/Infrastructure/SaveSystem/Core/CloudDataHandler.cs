using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using Infrastructure.SaveSystem.Data;

namespace Infrastructure.SaveSystem.Core
{
    /// <summary>
    /// Handler xử lý lưu trữ đám mây qua REST API.
    /// Tuân thủ Dependency Inversion: Nhận cấu hình từ bên ngoài, không hardcode.
    /// </summary>
    public class CloudDataHandler : IDataHandler
    {
        private readonly string _serverIP;
        private readonly string _userId;

        private string SaveUrl => $"http://{_serverIP}:3000/api/savegame";
        private string LoadUrl => $"http://{_serverIP}:3000/api/loadgame/{_userId}";

        public CloudDataHandler(string serverIP, string userId)
        {
            _serverIP = string.IsNullOrEmpty(serverIP) ? "localhost" : serverIP;
            _userId = string.IsNullOrEmpty(userId) ? SystemInfo.deviceUniqueIdentifier : userId;
            
            Debug.Log($"[CloudDataHandler] Initialized with ServerIP: {_serverIP}, UserID: {_userId}");
        }

        // Bỏ qua Load/Save đồng bộ vì WebRequest yêu cầu bất đồng bộ (Coroutine)
        public GameData Load() => null;
        public void Save(GameData data) {}
        public void Delete() {}

        /// <summary>
        /// Gửi dữ liệu lên Server. Trả về callback với trạng thái và thông báo.
        /// </summary>
        public IEnumerator SaveRoutine(GameData data, Action<bool, string> callback)
        {
            if (data == null)
            {
                callback?.Invoke(false, "Data is null");
                yield break;
            }

            string json = JsonUtility.ToJson(data);
            WWWForm form = new WWWForm();
            form.AddField("userID", _userId);
            form.AddField("inventoryJSON", json);

            using (UnityWebRequest www = UnityWebRequest.Post(SaveUrl, form))
            {
                www.timeout = 10; // Đặt timeout để không bị treo game nếu rớt mạng
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    callback?.Invoke(false, $"Network Error: {www.error}");
                }
                else
                {
                    callback?.Invoke(true, "Cloud Save Successful");
                }
            }
        }

        /// <summary>
        /// Tải dữ liệu từ Server. Trả về GameData và thông báo trạng thái.
        /// </summary>
        public IEnumerator LoadRoutine(Action<GameData, string> callback)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(LoadUrl))
            {
                www.timeout = 10; // Timeout 10s
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    callback?.Invoke(null, $"Network Error: {www.error}");
                }
                else
                {
                    try
                    {
                        string json = www.downloadHandler.text;
                        GameData data = JsonUtility.FromJson<GameData>(json);
                        callback?.Invoke(data, "Cloud Load Successful");
                    }
                    catch (Exception ex)
                    {
                        callback?.Invoke(null, $"Parse Error: {ex.Message}");
                    }
                }
            }
        }
    }
}
