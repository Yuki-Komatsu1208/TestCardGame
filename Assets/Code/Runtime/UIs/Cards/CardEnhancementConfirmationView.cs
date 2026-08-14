using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs.Cards
{
    /// <summary>レベルアップとMOD付与だけで使用する事前配置済みOK／NOモーダル。</summary>
    public sealed class CardEnhancementConfirmationView : MonoBehaviour
    {
        [SerializeField] private GameObject modalRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text targetText;
        [SerializeField] private TMP_Text enhancementText;
        [SerializeField] private TMP_Text errorText;
        [SerializeField] private Button okButton;
        [SerializeField] private Button noButton;

        private Action confirmed;
        private Action declined;

        private void Awake()
        {
            if (okButton != null) okButton.onClick.AddListener(Confirm);
            if (noButton != null) noButton.onClick.AddListener(Decline);
            Hide();
        }

        public void Show(string targetCards, string enhancement, Action onConfirmed, Action onDeclined)
        {
            confirmed = onConfirmed;
            declined = onDeclined;
            if (titleText != null) titleText.text = "強化します";
            if (targetText != null) targetText.text = targetCards ?? string.Empty;
            if (enhancementText != null) enhancementText.text = enhancement ?? string.Empty;
            if (errorText != null) errorText.gameObject.SetActive(false);
            if (modalRoot != null) modalRoot.SetActive(true);
        }

        public void ShowError(string message)
        {
            if (errorText == null) return;
            errorText.text = message ?? "強化を適用できませんでした。";
            errorText.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
        }

        private void Confirm() => confirmed?.Invoke();

        private void Decline()
        {
            Hide();
            declined?.Invoke();
        }
    }
}
