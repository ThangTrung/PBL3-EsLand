using Core.Contracts.Shared;
using Gameplay.Characters;
using Infrastructure.SaveSystem.Core;
using UI.Transition;
using UnityEngine;
using System.Collections;

namespace Gameplay.Building
{
    /// <summary>
    /// Script gắn vào Ngôi nhà để thực hiện chức năng Ngủ và Save Point qua tương tác chuột.
    /// </summary>
    public class HomeSavePoint : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private Vector3 respawnOffset = new Vector3(0, -1, 0);

        private Player _currentPlayer;
        private bool _isSleeping;

        public bool CanInteract(Character interactor) => interactor is Player && !_isSleeping;

        public float GetStaminaCost(Character interactor) => 0f;

        public void Interact(Character interactor)
        {
            if (interactor is Player player && !_isSleeping)
            {
                _currentPlayer = player;
                StartCoroutine(PerformSleepRoutine());
            }
        }

        private IEnumerator PerformSleepRoutine()
        {
            if (_currentPlayer == null) yield break;

            _isSleeping = true;

            // Phát tín hiệu để UIManager xử lý phần 'nhìn' (Mây, Fade)
            Core.Events.GameEvents.OnSleepRequested?.Invoke(this, _currentPlayer);

            // Chờ một khoảng thời gian dự phòng để tránh interact liên tục
            yield return new WaitForSecondsRealtime(4.0f); 
            
            _isSleeping = false;
        }

        /// <summary>
        /// Logic Gameplay thực tế khi ngủ (Sẽ được UIManager gọi khi màn hình đã tối/che mây)
        /// </summary>
        public void ExecuteSleepLogic(Player player)
        {
            if (player == null) return;

            // 1. Hồi máu
            player.Health.Heal(player.Health.MaxHealth);
            
            // 2. Hồi thể lực
            var survival = player.GetComponent<PlayerSurvivalController>();
            if (survival != null) survival.ModifyStamina(survival.MaxStamina);

            // 3. Cập nhật điểm hồi sinh
            var respawnCtrl = player.GetComponent<IRespawnable>();
            if (respawnCtrl != null)
            {
                respawnCtrl.SetRespawnPoint(transform.position + respawnOffset);
            }

            // 4. Save Game
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.SaveGame();
            }

        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + respawnOffset, 0.3f); // Vẽ điểm hồi sinh
        }
    }
}
