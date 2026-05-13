using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.ItemActions
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
            if (menuPanel == null) menuPanel = transform.Find("MenuPanel")?.gameObject;
            if (blockerPanel == null) blockerPanel = transform.Find("Blocker")?.gameObject;

            if (menuPanel) menuPanel.SetActive(false);
            if (blockerPanel) blockerPanel.SetActive(false);

            if (useButton) useButton.onClick.AddListener(() => OnUseClicked?.Invoke());
            if (dropButton) dropButton.onClick.AddListener(() => OnDropClicked?.Invoke());
            if (equipButton) equipButton.onClick.AddListener(() => OnEquipClicked?.Invoke());
            if (unequipButton) unequipButton.onClick.AddListener(() => OnUnequipClicked?.Invoke());
        }

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

