using System.Collections;
using UnityEngine;
using Core.Contracts.Shared;
using TMPro;

namespace UI.Transition
{
    public class CloudTransitionUI : MonoBehaviour, ITransitionService
    {
        [Header("References")]
        [SerializeField] private Animator cloudAnimator;
        [SerializeField] private TextMeshProUGUI statusText;
        
        private const string CloudCloseState = "Cloud_FadeIn";
        private const string CloudOpenState = "Cloud_FadeOut";

        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            if (cloudAnimator == null) cloudAnimator = GetComponent<Animator>();
            cloudAnimator.updateMode = AnimatorUpdateMode.UnscaledTime; 
            gameObject.SetActive(false);
        }

        // Hàm này dùng để HIDE scene hiện tại (Mây bay VÀO che lại)
        public IEnumerator FadeOut(float duration, string message = "")
        {
            IsTransitioning = true;
            
            // 1. Bật UI lên trước
            gameObject.SetActive(true);
            if (statusText) statusText.text = message;
            yield return null; // Đợi 1 frame để UI khởi tạo

            // 2. Chạy Animation mây bay VÀO che kín
            if (!cloudAnimator) yield break;
            cloudAnimator.Play(CloudCloseState);
            yield return new WaitForSecondsRealtime(duration);
        }

        // Hàm này dùng để SHOW scene mới (Mây bay RA mở ra)
        public IEnumerator FadeIn(float duration)
        {
            // 1. Chạy Animation mây bay RA
            if (cloudAnimator)
            {
                cloudAnimator.Play(CloudOpenState);
                yield return new WaitForSecondsRealtime(duration);
            }

            // 2. Đợi mây bay ra hết màn hình thì mới tắt Canvas đi
            gameObject.SetActive(false);
            IsTransitioning = false;
        }
    }
}