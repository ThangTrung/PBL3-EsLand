using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Script.Inventory.UI
{
    public class ItemActionMenuUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject menuPanel;

        [Header("Buttons")]
        [SerializeField] private Button useButton;
        [SerializeField] private Button dropButton;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button unequipButton;
        
        public event Action OnUseClicked;
        public event Action OnDropClicked;
        public event Action OnEquipClicked;
        public event Action OnUnequipClicked;

        private void Awake()
        {
            menuPanel.SetActive(false);
            useButton.onClick.AddListener(() => OnUseClicked?.Invoke());
            dropButton.onClick.AddListener(() => OnDropClicked?.Invoke());
            equipButton.onClick.AddListener(() => OnEquipClicked?.Invoke());
            unequipButton.onClick.AddListener(() => OnUnequipClicked?.Invoke());
        }
        
        public void Show(Vector3 worldPos, bool showUse, bool showEquip, bool showUnequip)
        {
            useButton.gameObject.SetActive(showUse);
            dropButton.gameObject.SetActive(true);
            equipButton.gameObject.SetActive(showEquip);
            unequipButton.gameObject.SetActive(showUnequip);
            menuPanel.transform.position = worldPos;
            menuPanel.SetActive(true);
        }

        public void Hide()
        {
            if(menuPanel) menuPanel.SetActive(false);
        }

        private void Update()
        {
            if (menuPanel && menuPanel.activeSelf &&
                Input.GetMouseButtonDown(0) &&
                !EventSystem.current.IsPointerOverGameObject())
            {
                Hide();
            }
        }
    }
}