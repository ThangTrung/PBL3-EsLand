using System;

namespace Core.Events
{
    public static class BossHealthEventChannel
    {
        /// <summary>
        /// Triggered when boss health changes.
        /// Params: BossName, CurrentHealth, MaxHealth
        /// </summary>
        public static event Action<string, float, float> OnBossHealthUpdated;

        /// <summary>
        /// Triggered when a boss is defeated.
        /// </summary>
        public static event Action OnBossDefeated;

        public static void RaiseBossHealthUpdated(string name, float current, float max)
        {
            if (string.IsNullOrEmpty(name)) return;
            OnBossHealthUpdated?.Invoke(name, current, max);
        }

        public static void RaiseBossDefeated()
        {
            OnBossDefeated?.Invoke();
        }
    }
}