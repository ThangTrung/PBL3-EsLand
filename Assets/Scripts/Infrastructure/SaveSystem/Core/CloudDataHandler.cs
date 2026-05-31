using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using Infrastructure.SaveSystem.Data;

namespace Infrastructure.SaveSystem.Core
{
    /// <summary>
    /// CloudDataHandler - Implement IDataHandler for Cloud Save.
    /// Chú ý: Vì IDataHandler hiện tại đang dùng thiết kế đồng bộ (Load/Save), 
    /// nhưng WebRequest là bất đồng bộ, trong bản demo này tôi sẽ dùng chuỗi JSON 
    /// và giả lập luồng để phù hợp interface.
    /// </summary>
    public class CloudDataHandler : IDataHandler
    {
        private string saveUrl = "http://localhost:3000/api/savegame";
        private string loadUrl = "http://localhost:3000/api/loadgame/";
        private string userId = "1"; // Giả định UserID = 1 cho demo

        public GameData Load()
        {
            // Đồng bộ: Cloud Load cần được gọi qua LoadRoutine (Async)
            return null; 
        }

        public void Save(GameData data)
        {
            // Đồng bộ: Cloud Save cần được gọi qua SaveRoutine (Async)
        }

        // Helper cho SaveLoadManager gọi (Bất đồng bộ)
        public IEnumerator SaveRoutine(GameData data, Action<bool> callback)
        {
            string json = JsonUtility.ToJson(data);
            WWWForm form = new WWWForm();
            form.AddField("userID", userId);
            form.AddField("inventoryJSON", json);

            using (UnityWebRequest www = UnityWebRequest.Post(saveUrl, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(false);
                }
                else
                {
                    callback?.Invoke(true);
                }
            }
        }

        public IEnumerator LoadRoutine(Action<GameData> callback)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(loadUrl + userId))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(null);
                }
                else
                {
                    string json = www.downloadHandler.text;
                    GameData data = JsonUtility.FromJson<GameData>(json);
                    callback?.Invoke(data);
                }
            }
        }
    }
}
