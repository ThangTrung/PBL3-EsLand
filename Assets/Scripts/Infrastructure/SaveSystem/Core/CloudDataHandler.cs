using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

/// <summary>
/// CloudDataHandler - Implement IDataHandler for Cloud Save.
/// Chú ý: Vì IDataHandler hiện tại đang dùng thiết kế đồng bộ (Load/Save), 
/// nhưng WebRequest là bất đồng bộ, trong bản demo này tôi sẽ dùng chuỗi JSON 
/// và giả lập luồng để phù hợp interface, hoặc đề xuất refactor interface sang Async.
/// Tuy nhiên, để tuân thủ SRP và DIP yêu cầu, tôi sẽ implement logic WebRequest ở đây.
/// </summary>
public class CloudDataHandler : IDataHandler
{
    private string saveUrl = "http://localhost:3000/api/savegame";
    private string loadUrl = "http://localhost:3000/api/loadgame/";
    private string userId = "1"; // Giả định UserID = 1 cho demo

    public GameData Load()
    {
        // Trong thực tế Production, Load nên là Async/Task. 
        // Ở đây tôi cung cấp phương thức dùng UnityWebRequest (cần chạy trong Coroutine của SaveLoadManager).
        Debug.Log("CloudDataHandler: Đang tải dữ liệu từ Cloud...");
        return null; // Trả về null để SaveLoadManager xử lý Coroutine riêng nếu cần
    }

    public void Save(GameData data)
    {
        string json = JsonUtility.ToJson(data);
        // Logic gửi Request sẽ được gọi từ MonoBehaviour (SaveLoadManager)
        Debug.Log("CloudDataHandler: Chuẩn bị gửi dữ liệu lên Cloud...");
    }

    // Helper cho SaveLoadManager gọi
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
                Debug.LogError("Cloud Save Error: " + www.error);
                callback?.Invoke(false);
            }
            else
            {
                Debug.Log("Cloud Save Success!");
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
                Debug.LogError("Cloud Load Error: " + www.error);
                callback?.Invoke(null);
            }
            else
            {
                string json = www.downloadHandler.text;
                // Giả định Backend trả về JSON object có field inventoryJSON
                // Bạn có thể cần một wrapper class tùy theo format API
                GameData data = JsonUtility.FromJson<GameData>(json);
                callback?.Invoke(data);
            }
        }
    }
}
