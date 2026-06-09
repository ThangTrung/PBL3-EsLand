using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

using Infrastructure.Networking;

namespace Infrastructure.SaveSystem.Core
{
    /// <summary>
    /// CloudAuthService - Xử lý đăng nhập và xác thực với Backend.
    /// </summary>
    public class CloudAuthService
    {
        private static string authUrl => NetworkSettings.Instance != null 
            ? NetworkSettings.Instance.LoginUrl 
            : "http://localhost:3000/api/auth/login";
        
        public static string CurrentUserID { get; private set; }
        public static bool IsLoggedIn => !string.IsNullOrEmpty(CurrentUserID);

        [Serializable]
        private class AuthRequest
        {
            public string username;
            public string password;
        }

        [Serializable]
        private class AuthResponse
        {
            public bool success;
            public int userID; // Backend trả về INT
            public string message;
        }

        public static IEnumerator Login(string username, string password, Action<bool, string> callback)
        {
            AuthRequest requestData = new AuthRequest { username = username, password = password };
            string json = JsonUtility.ToJson(requestData);

            using (UnityWebRequest www = new UnityWebRequest(authUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError($"[CloudAuthService] Network Error: {www.error}");
                    callback?.Invoke(false, "Không thể kết nối Server (Kiểm tra IP)");
                }
                else
                {
                    try
                    {
                        string responseText = www.downloadHandler.text;
                        AuthResponse response = JsonUtility.FromJson<AuthResponse>(responseText);
                        
                        if (www.result == UnityWebRequest.Result.Success && response != null && response.success)
                        {
                            CurrentUserID = response.userID.ToString();
                            callback?.Invoke(true, response.message);
                        }
                        else
                        {
                            string msg = response != null && !string.IsNullOrEmpty(response.message) 
                                ? response.message 
                                : $"Lỗi Server ({www.responseCode})";
                            callback?.Invoke(false, msg);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[CloudAuthService] Response Error: {ex.Message}. Raw: {www.downloadHandler.text}");
                        callback?.Invoke(false, "Lỗi phản hồi từ hệ thống");
                    }
                }
            }
        }

        public static void Logout()
        {
            CurrentUserID = null;
        }
    }
}
