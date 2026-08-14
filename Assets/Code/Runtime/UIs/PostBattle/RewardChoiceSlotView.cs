using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs.PostBattle
{
    /// <summary>事前配置された報酬1枠の表示と選択通知だけを担当する。</summary>
    public sealed class RewardChoiceSlotView : MonoBehaviour
    {
        [SerializeField] private Image frameImage;
        [SerializeField] private TMP_Text typeText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text unavailableText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Color levelUpColor = new(0.92f, 0.70f, 0.16f, 1f);
        [SerializeField] private Color healColor = new(0.25f, 0.62f, 0.29f, 1f);
        [SerializeField] private Color modifierColor = new(0.68f, 0.16f, 0.14f, 1f);

        private int choiceId;
        private Action<int> selected;

        private void Awake()
        {
            if (selectButton != null) selectButton.onClick.AddListener(NotifySelected);
        }

        private void OnDestroy()
        {
            if (selectButton != null) selectButton.onClick.RemoveListener(NotifySelected);
        }

        public void Render(RewardChoiceViewModel model, Action<int> onSelected)
        {
            choiceId = model.ChoiceId;
            selected = onSelected;
            if (frameImage != null) frameImage.color = ResolveColor(model.Kind);
            if (typeText != null) typeText.text = ResolveLabel(model.Kind);
            if (titleText != null) titleText.text = model.Title;
            if (descriptionText != null) descriptionText.text = model.Description;
            if (unavailableText != null)
            {
                unavailableText.text = model.UnavailableReason;
                unavailableText.gameObject.SetActive(!model.IsSelectable);
            }
            if (selectButton != null) selectButton.interactable = model.IsSelectable;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            selected = null;
            gameObject.SetActive(false);
        }

        private void NotifySelected() => selected?.Invoke(choiceId);

        private Color ResolveColor(RewardDisplayKind kind)
        {
            return kind switch
            {
                RewardDisplayKind.Heal => healColor,
                RewardDisplayKind.Modifier => modifierColor,
                _ => levelUpColor
            };
        }

        private static string ResolveLabel(RewardDisplayKind kind)
        {
            return kind switch
            {
                RewardDisplayKind.Heal => "HP回復",
                RewardDisplayKind.Modifier => "MOD付与",
                _ => "レベルアップ"
            };
        }

        public bool ValidateReferences() => frameImage != null && typeText != null && titleText != null
            && descriptionText != null && unavailableText != null && selectButton != null;
    }
}
