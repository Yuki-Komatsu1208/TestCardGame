using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs.Cards
{
    /// <summary>一覧専用の事前配置スロット。ゲーム状態を保持せず、入力はTokenだけを通知する。</summary>
    public sealed class CardListSlotView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text disabledReasonText;
        [SerializeField] private GameObject selectionMark;
        [SerializeField] private GameObject focusMark;

        private CardDisplayModel model;
        private Action<CardSelectionToken> clicked;

        public CardSelectionToken Token => model != null ? model.Token : new CardSelectionToken(-1);

        private void Awake()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(NotifyClicked);
                button.onClick.AddListener(NotifyClicked);
            }
        }

        public void Bind(CardDisplayModel displayModel, Action<CardSelectionToken> onClicked)
        {
            model = displayModel;
            clicked = onClicked;
            gameObject.SetActive(model != null);
            if (model == null)
                return;

            if (nameText != null) nameText.text = model.Name;
            if (levelText != null) levelText.text = $"Lv.{model.CurrentLevel}";
            if (costText != null) costText.text = $"COST {model.CurrentCost}";
            if (disabledReasonText != null)
            {
                disabledReasonText.text = model.IsSelectable ? string.Empty : model.DisabledReason;
                disabledReasonText.gameObject.SetActive(!model.IsSelectable);
            }
            if (background != null)
                background.color = model.IsSelectable ? new Color(0.82f, 0.75f, 0.60f, 1f) : new Color(0.38f, 0.38f, 0.38f, 1f);
            SetState(false, false);
        }

        public void Clear()
        {
            model = null;
            clicked = null;
            gameObject.SetActive(false);
        }

        public void SetState(bool selected, bool focused)
        {
            if (selectionMark != null) selectionMark.SetActive(selected);
            if (focusMark != null) focusMark.SetActive(focused);
        }

        private void NotifyClicked()
        {
            if (model != null)
                clicked?.Invoke(model.Token);
        }
    }
}
