using System;
using Core.Contracts.Inventory;
using Data.Loot;


namespace Core.Events
{
    /// <summary>
    /// Event Bus trung tâm xử lý các sự kiện toàn cục trong game.
    /// Giúp tách biệt (decouple) các module như Core, Gameplay, UI.
    /// </summary>
    public static class GameEvents
    {
        /// <summary>
        /// Phát ra khi Player được sinh ra và đã sẵn sàng dữ liệu.
        /// Payload: IInventoryHolder của Player.
        /// </summary>
        public static Action<IInventoryHolder> OnPlayerReady;
        /// <summary>\r\n
        /// Phát ra khi một quái vật chết.
        /// Payload: instance của quái vật.
        /// </summary>
        public static Action<Gameplay.AI.EnemyBase> OnEnemyDied;
        /// <summary>
        /// Phát ra khi quái vật rơi vật phẩm.
        /// Payload: Thông tin vật phẩm và vị trí rơi.
        /// </summary>
        public static Action<Data.Loot.LootDropData> OnEnemyDroppedLoot;

        public static void InvokeEnemyDroppedLoot(Data.Loot.LootDropData data)
        {
            OnEnemyDroppedLoot?.Invoke(data);
        }

        // Sự kiện yêu cầu ngủ (để UIManager xử lý chuyển cảnh)
        public static Action<Gameplay.Building.HomeSavePoint, Gameplay.Characters.Player> OnSleepRequested;

        /// <summary>
        /// Phát ra khi người chơi chiến thắng trò chơi (thoát đảo).
        /// </summary>
        public static Action OnVictory;

        public static void RaiseVictory()
        {
            OnVictory?.Invoke();
        }

        /// <summary>
        /// Phát ra khi một bảng UI độc quyền được mở (ví dụ: Settings, Guide).
        /// Các bảng UI khác đang mở nên lắng nghe sự kiện này để tự đóng lại.
        /// Payload: GameObject của bảng UI vừa mở (để nó không tự đóng chính nó).
        /// </summary>
        public static Action<UnityEngine.GameObject> OnExclusiveUIOpened;
    }
}
