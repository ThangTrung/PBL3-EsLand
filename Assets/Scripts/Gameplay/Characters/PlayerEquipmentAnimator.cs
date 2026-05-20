using Core.Contracts.Equipment;
using Data.Equipment;
using UnityEngine;

namespace Gameplay.Characters
{
    public class PlayerEquipmentAnimator : MonoBehaviour
    {
        // Nhớ kéo file Animator tay không vào đây ngoài Inspector nhé!
        [SerializeField] private RuntimeAnimatorController baseAnimator;

        private Character _facade;

        private void Awake()
        {
            _facade = GetComponentInParent<Character>();

            // ĐÃ XÓA đoạn if tự động lấy baseAnimator.
            // Ép buộc phải dùng cái baseAnimator được kéo vào từ Inspector để không bị LoadGame ghi đè.
        }

        private void Start()
        {
            if (_facade != null && _facade.EquipmentManager != null)
            {
                // 1. Đăng ký lắng nghe sự kiện
                _facade.EquipmentManager.OnItemEquipped += HandleItemEquipped;
                _facade.EquipmentManager.OnItemUnequipped += HandleItemUnequipped;

                // 2. ĐỒNG BỘ BÙ (Catch-up Sync) khi vừa vào game
                var mainHandItem = _facade.EquipmentManager.GetEquippedItem(EquipSlot.MainHand);
                if (mainHandItem != null)
                {
                    // Nếu load save vào thấy đang cầm hàng nóng -> Ép đổi hình luôn
                    HandleItemEquipped(EquipSlot.MainHand, mainHandItem);
                }
                else
                {
                    // Nếu tay không -> Ép về hình dáng tay không
                    ResetAnimator();
                }
            }
        }

        private void OnDestroy()
        {
            if (_facade == null || _facade.EquipmentManager == null) return;
            _facade.EquipmentManager.OnItemEquipped -= HandleItemEquipped;
            _facade.EquipmentManager.OnItemUnequipped -= HandleItemUnequipped;
        }

        private void HandleItemEquipped(EquipSlot slot, IEquippable item)
        {
            if (slot != EquipSlot.MainHand || item is not Data.Equipment.Equipment equipment || _facade == null || _facade.Animator == null)
                return;

            if (equipment.overrideController)
                _facade.Animator.runtimeAnimatorController = equipment.overrideController;
            else
                ResetAnimator();
        }

        private void HandleItemUnequipped(EquipSlot slot, IEquippable item)
        {
            if (slot == EquipSlot.MainHand && _facade != null && _facade.Animator != null)
                ResetAnimator();
        }

        private void ResetAnimator()
        {
            if (_facade != null && _facade.Animator != null && baseAnimator != null)
                _facade.Animator.runtimeAnimatorController = baseAnimator;
        }
    }
}