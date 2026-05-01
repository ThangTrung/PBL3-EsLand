using Core.Contracts.Equipment;
using Data.Equipment;
using UnityEngine;

namespace Gameplay.Characters
{
    public class PlayerEquipmentAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private RuntimeAnimatorController baseAnimator;

        private IEquipmentController _equipmentController;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (animator != null && baseAnimator == null)
            {
                baseAnimator = animator.runtimeAnimatorController;
            }

            _equipmentController = GetComponent<IEquipmentController>();
        }

        private void Start()
        {
            if (_equipmentController != null)
            {
                _equipmentController.OnItemEquipped += HandleItemEquipped;
                _equipmentController.OnItemUnequipped += HandleItemUnequipped;
            }
        }

        private void OnDestroy()
        {
            if (_equipmentController == null) return;
            _equipmentController.OnItemEquipped -= HandleItemEquipped;
            _equipmentController.OnItemUnequipped -= HandleItemUnequipped;
        }

        private void HandleItemEquipped(EquipSlot slot, IEquippable item)
        {
            if (slot != EquipSlot.MainHand || item is not Data.Equipment.Equipment equipment || animator == null)
                return;

            if (equipment.overrideController)
                animator.runtimeAnimatorController = equipment.overrideController;
            else
                ResetAnimator();
        }

        private void HandleItemUnequipped(EquipSlot slot, IEquippable item)
        {
            if (slot == EquipSlot.MainHand && animator != null)
                ResetAnimator();
        }

        private void ResetAnimator()
        {
            if (animator != null && baseAnimator != null)
                animator.runtimeAnimatorController = baseAnimator;
        }
    }
}
