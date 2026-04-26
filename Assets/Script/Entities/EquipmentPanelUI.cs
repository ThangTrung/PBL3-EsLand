using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Script.Items;
using Script.Entities;
using Script.Interfaces;

namespace Script.UI
{
    public class EquipmentPanelUI : MonoBehaviour
    {
        [Header("Giao diện UI")]
        public GameObject equipmentPanel;

        [Header("5 Ô Icon Trang Bị")]
        public Image attackStoneIcon;
        public Image healthStoneIcon;
        public Image speedStoneIcon;
        public Image defenseStoneIcon;
        public Image mainHandIcon;

        [Header("Tham chiếu Logic")]
        public EquipmentManager equipmentManager;
        public Animator playerAnimator;

        [Header("Khóa Di Chuyển")]
        public MonoBehaviour playerMovementScript;

        private RuntimeAnimatorController _defaultAnimator;
        private Dictionary<EquipSlot, Sprite> _defaultIcons = new Dictionary<EquipSlot, Sprite>();

        // Dùng Dictionary thay cho switch-case để chuẩn OCP (SOLID)
        private Dictionary<EquipSlot, Image> _slotImages;

        private void Start()
        {
            // Thiết lập ánh xạ tự động cho các khe đồ
            _slotImages = new Dictionary<EquipSlot, Image>
            {
                { EquipSlot.AttackStone, attackStoneIcon },
                { EquipSlot.HealthStone, healthStoneIcon },
                { EquipSlot.SpeedStone, speedStoneIcon },
                { EquipSlot.DefenseStone, defenseStoneIcon },
                { EquipSlot.MainHand, mainHandIcon }
            };

            if (equipmentPanel != null) equipmentPanel.SetActive(false);

            if (playerAnimator != null)
                _defaultAnimator = playerAnimator.runtimeAnimatorController;

            SaveDefaultIcon(EquipSlot.AttackStone, attackStoneIcon);
            SaveDefaultIcon(EquipSlot.HealthStone, healthStoneIcon);
            SaveDefaultIcon(EquipSlot.SpeedStone, speedStoneIcon);
            SaveDefaultIcon(EquipSlot.DefenseStone, defenseStoneIcon);

            if (mainHandIcon != null)
            {
                mainHandIcon.sprite = null;
                mainHandIcon.color = new Color(1, 1, 1, 0f);
            }

            if (equipmentManager != null)
            {
                equipmentManager.OnItemEquipped += HandleItemEquipped;
                equipmentManager.OnItemUnequipped += HandleItemUnequipped;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E) && equipmentPanel != null)
            {
                bool isOpening = !equipmentPanel.activeSelf;
                equipmentPanel.SetActive(isOpening);

                // Khóa hoặc mở khóa script di chuyển
                if (playerMovementScript != null)
                {
                    playerMovementScript.enabled = !isOpening;
                }
            }
        }

        private void SaveDefaultIcon(EquipSlot slot, Image iconImage)
        {
            if (iconImage != null && iconImage.sprite != null)
            {
                _defaultIcons[slot] = iconImage.sprite;
                iconImage.color = new Color(1, 1, 1, 0.3f);
            }
        }

        private void HandleItemEquipped(EquipSlot slot, IEquippable equippableItem)
        {
            if (equippableItem is Equipment item)
            {
                if (_slotImages.TryGetValue(slot, out Image targetSlot) && targetSlot != null)
                {
                    targetSlot.sprite = item.Icon;
                    targetSlot.color = new Color(1, 1, 1, 1f);
                }

                if (slot == EquipSlot.MainHand && item.overrideController != null && playerAnimator != null)
                {
                    playerAnimator.runtimeAnimatorController = item.overrideController;
                }
            }
        }

        private void HandleItemUnequipped(EquipSlot slot, IEquippable item)
        {
            if (_slotImages.TryGetValue(slot, out Image targetSlot) && targetSlot != null)
            {
                if (slot == EquipSlot.MainHand)
                {
                    targetSlot.sprite = null;
                    targetSlot.color = new Color(1, 1, 1, 0f);
                }
                else
                {
                    targetSlot.sprite = _defaultIcons.ContainsKey(slot) ? _defaultIcons[slot] : null;
                    targetSlot.color = new Color(1, 1, 1, 0.3f);
                }
            }

            if (slot == EquipSlot.MainHand && _defaultAnimator != null && playerAnimator != null)
            {
                playerAnimator.runtimeAnimatorController = _defaultAnimator;
            }
        }
    }
}