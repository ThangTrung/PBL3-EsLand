using System;
using Core.Contracts.Inventory;

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
    }
}
