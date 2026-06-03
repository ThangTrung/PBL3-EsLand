using System;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Quản lý thời gian trong game (Day/Night Cycle).
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [Header("Time Settings")]
        [Tooltip("Số phút thực tế cho 1 ngày trong game")]
        [SerializeField] private float dayLengthInMinutes = 10f;
        [SerializeField] private float startTimeHour = 6f;

        [Header("Runtime Data")]
        [SerializeField, Range(0, 24)] private float currentTime;
        private int _lastHour = -1;

        public float CurrentTime => currentTime;
        public bool IsNight => currentTime > 18f || currentTime < 6f;

        // Sự kiện khi mỗi giờ trong game trôi qua
        public static event Action<int> OnHourChanged;
        // Sự kiện báo hiệu bắt đầu ngày mới
        public static event Action OnNewDay;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                currentTime = startTimeHour;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            // Tính toán tốc độ trôi của thời gian
            // 24 giờ game / (dayLength * 60 giây thực)
            float timeMultiplier = 24f / (dayLengthInMinutes * 60f);
            currentTime += Time.deltaTime * timeMultiplier;

            if (currentTime >= 24f)
            {
                currentTime = 0f;
                OnNewDay?.Invoke();
            }

            // Kiểm tra và phát sự kiện đổi giờ
            int currentHourInt = Mathf.FloorToInt(currentTime);
            if (currentHourInt != _lastHour)
            {
                _lastHour = currentHourInt;
                OnHourChanged?.Invoke(currentHourInt);
            }
        }

        /// <summary>
        /// Nhảy thời gian đến sáng hôm sau (dùng cho tính năng ngủ).
        /// </summary>
        public void SkipToMorning(float morningHour = 6f)
        {
            currentTime = morningHour;
            _lastHour = Mathf.FloorToInt(currentTime);
            OnHourChanged?.Invoke(_lastHour);
            OnNewDay?.Invoke();
            Debug.Log("<color=lightblue>[Time]</color> Đã nhảy thời gian đến sáng hôm sau.");
        }

        public string GetFormattedTime()
        {
            int hours = Mathf.FloorToInt(currentTime);
            int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
            return string.Format("{0:00}:{1:02}", hours, minutes);
        }
    }
}
