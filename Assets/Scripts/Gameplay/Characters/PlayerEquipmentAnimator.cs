using Core.Contracts.Equipment;
using Data.Equipment;
using UnityEngine;

namespace Gameplay.Characters
{
    public class PlayerEquipmentAnimator : MonoBehaviour
    {
        [SerializeField] private RuntimeAnimatorController baseAnimator;

        private Character _facade;

        private void Awake()
        {
            _facade = GetComponentInParent<Character>();

            if (_facade != null && _facade.Animator != null && baseAnimator == null)
            {
                baseAnimator = _facade.Animator.runtimeAnimatorController;
            }
        }

        private void Start()
        {
            if (_facade != null && _facade.EquipmentManager != null)
            {
                _facade.EquipmentManager.OnItemEquipped += HandleItemEquipped;
                _facade.EquipmentManager.OnItemUnequipped += HandleItemUnequipped;
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
