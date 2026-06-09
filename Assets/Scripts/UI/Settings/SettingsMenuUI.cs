using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.Settings
{
    public class SettingsMenuUI : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject menuPanel;

        [Header("Button Container")]
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private List<Button> subButtons; // Danh sách 4 nút con có sẵn

        public event Action<SettingsActionData> OnActionClicked;

        public bool IsVisible => menuPanel != null && menuPanel.activeSelf;

        private void Awake()
        {
            if (menuPanel) menuPanel.SetActive(false);

            // Gán sự kiện cho các nút con
            for (int i = 0; i < subButtons.Count; i++)
            {
                int index = i; // Closure
                subButtons[i].onClick.AddListener(() => HandleButtonClick(index));
            }
        }

        private void Update()
        {
            // Tự động đóng menu nếu người chơi click chuột trái ra ngoài vùng UI (bấm vào map/nhân vật)
            if (IsVisible && Input.GetMouseButtonDown(0))
            {
                if (UnityEngine.EventSystems.EventSystem.current != null && 
                    !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    Hide();
                }
            }
        }

        private void OnEnable()
        {
            Core.Events.GameEvents.OnExclusiveUIOpened += HandleExclusiveUIOpened;
        }

        private void OnDisable()
        {
            Core.Events.GameEvents.OnExclusiveUIOpened -= HandleExclusiveUIOpened;
        }

        private void HandleExclusiveUIOpened(GameObject source)
        {
            if (source != this.gameObject && IsVisible)
            {
                Hide();
            }
        }

        private List<SettingsActionData> _currentActions;

        public void Setup(List<SettingsActionData> actions, Vector3 position)
        {
            _currentActions = actions;

            // Cập nhật vị trí panel (tương tự Action Menu)
            if (menuPanel.TryGetComponent<RectTransform>(out var rect))
            {
                rect.position = position;
            }

            // Hiển thị/Ẩn các nút dựa trên data
            for (int i = 0; i < subButtons.Count; i++)
            {
                if (i < actions.Count)
                {
                    subButtons[i].gameObject.SetActive(true);
                    
                    // Cập nhật label/icon nếu có
                    var text = subButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (text) text.text = actions[i].label;

                    var image = subButtons[i].transform.Find("Icon")?.GetComponent<Image>();
                    if (image && actions[i].icon != null) image.sprite = actions[i].icon;
                }
                else
                {
                    subButtons[i].gameObject.SetActive(false);
                }
            }

            menuPanel.SetActive(true);

            // Báo cho các UI khác biết tôi vừa mở, vui lòng tự đóng lại
            Core.Events.GameEvents.OnExclusiveUIOpened?.Invoke(this.gameObject);
        }

        public void Hide()
        {
            menuPanel.SetActive(false);
        }

        private void HandleButtonClick(int index)
        {
            if (index >= 0 && index < _currentActions.Count)
            {
                OnActionClicked?.Invoke(_currentActions[index]);
            }
            Hide();
        }
    }
}
