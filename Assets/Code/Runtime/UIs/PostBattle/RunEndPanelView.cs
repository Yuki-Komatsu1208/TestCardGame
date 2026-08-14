using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs.PostBattle
{
    /// <summary>RUN終了内容を表示し、新RUN開始要求だけを通知する。</summary>
    public sealed class RunEndPanelView : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text actionLabelText;
        [SerializeField] private Button actionButton;
        [SerializeField] private Image accentImage;
        [SerializeField] private Color victoryColor = new(0.84f, 0.65f, 0.16f, 1f);
        [SerializeField] private Color deathColor = new(0.58f, 0.09f, 0.08f, 1f);
        [SerializeField] private Color fatalColor = new(0.92f, 0.58f, 0.08f, 1f);

        public event Action RestartRequested;

        private void Awake() => actionButton?.onClick.AddListener(NotifyRestart);
        private void OnDestroy() => actionButton?.onClick.RemoveListener(NotifyRestart);

        public void Show(RunEndViewModel model)
        {
            if (messageText != null) messageText.text = model.Message;
            if (actionLabelText != null) actionLabelText.text = model.ButtonLabel;
            if (accentImage != null) accentImage.color = ResolveColor(model.Kind);
            if (actionButton != null) actionButton.interactable = true;
            panelRoot?.SetActive(true);
        }

        public void SetInteractionEnabled(bool enabled)
        {
            if (actionButton != null) actionButton.interactable = enabled;
        }

        public void Hide() => panelRoot?.SetActive(false);
        private void NotifyRestart() => RestartRequested?.Invoke();

        private Color ResolveColor(RunEndKind kind)
        {
            return kind switch
            {
                RunEndKind.Death => deathColor,
                RunEndKind.FatalError => fatalColor,
                _ => victoryColor
            };
        }

        public bool ValidateReferences() => panelRoot != null && messageText != null && actionLabelText != null
            && actionButton != null && accentImage != null;
    }
}
