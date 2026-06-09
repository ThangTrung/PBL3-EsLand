using UnityEngine;

namespace UI.Cursor
{
    public enum CursorType
    {
        Normal,
        Pointer,
        Forbidden
    }

    /// <summary>
    /// Quản lý việc thay đổi con trỏ chuột tập trung.
    /// </summary>
    public class CursorManager : MonoBehaviour
    {
        public static CursorManager Instance { get; private set; }

        [Header("Cursor Textures")]
        [SerializeField] private Texture2D normalCursor;
        [SerializeField] private Texture2D pointerCursor;
        [SerializeField] private Texture2D forbiddenCursor;

        [Header("Hotspots")]
        [SerializeField] private Vector2 normalHotspot = Vector2.zero;
        [SerializeField] private Vector2 pointerHotspot = new Vector2(10, 0); // Ví dụ: đầu ngón tay
        [SerializeField] private Vector2 forbiddenHotspot = Vector2.zero;

        private CursorType _currentType = CursorType.Normal;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SetCursor(CursorType.Normal);
        }

        public void SetCursor(CursorType type)
        {
            _currentType = type;
            
            Texture2D tex = null;
            Vector2 hotspot = Vector2.zero;

            switch (type)
            {
                case CursorType.Normal:
                    tex = normalCursor;
                    hotspot = normalHotspot;
                    break;
                case CursorType.Pointer:
                    tex = pointerCursor;
                    hotspot = pointerHotspot;
                    break;
                case CursorType.Forbidden:
                    tex = forbiddenCursor;
                    hotspot = forbiddenHotspot;
                    break;
            }

            UnityEngine.Cursor.SetCursor(tex, hotspot, CursorMode.Auto);
        }

        public void SetNormalCursor() => SetCursor(CursorType.Normal);
        public void SetPointerCursor() => SetCursor(CursorType.Pointer);
        public void SetForbiddenCursor() => SetCursor(CursorType.Forbidden);
    }
}
