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
        [SerializeField] private GameObject blockerPanel;
        
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
        
        // ReSharper disable Unity.PerformanceAnalysis
        public void Show(Vector3 screenPos, bool showUse, bool showEquip, bool showUnequip)
        {
            useButton.gameObject.SetActive(showUse);
            dropButton.gameObject.SetActive(true);
            equipButton.gameObject.SetActive(showEquip);
            unequipButton.gameObject.SetActive(showUnequip);
            
            var menuRect = menuPanel.GetComponent<RectTransform>();
            var parentRect = menuRect.parent.GetComponent<RectTransform>();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, 
                screenPos, 
                null, 
                out var localPoint
            );
            menuRect.localPosition = localPoint;
            menuPanel.SetActive(true);
            blockerPanel.SetActive(true);
        }

        public void Hide()
        {
            menuPanel.SetActive(false);
            blockerPanel.SetActive(false);
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