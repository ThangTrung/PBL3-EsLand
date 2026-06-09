using System.Collections.Generic;
using UnityEngine;

namespace UI.Settings
{
    public enum SettingsActionType
    {
        Save,
        Load,
        Exit,
        Settings,
        Help,
        Custom,
        LogOut
    }

    [System.Serializable]
    public class SettingsActionData
    {
        public string label;
        public Sprite icon;
        public SettingsActionType actionType;
        public string customID; // Dùng nếu actionType là Custom
    }

    [CreateAssetMenu(fileName = "SettingsConfig", menuName = "UI/Settings Config")]
    public class SettingsActionConfig : ScriptableObject
    {
        public string sceneName; // Để trống nếu là config mặc định
        public List<SettingsActionData> actions = new List<SettingsActionData>();
    }
}
