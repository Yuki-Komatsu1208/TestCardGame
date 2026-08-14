using System;
using TMPro;
using UnityEngine;

namespace TestCardGame.UIs.PostBattle
{
    /// <summary>固定3枠の報酬表示と選択ID通知を担当する。</summary>
    public sealed class RewardChoicePanelView : MonoBehaviour
    {
        public const int RequiredSlotCount = 3;

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private RewardChoiceSlotView[] slots = Array.Empty<RewardChoiceSlotView>();

        public event Action<int> RewardSelected;

        public void Show(RewardChoiceViewModel[] models)
        {
            if (models == null || models.Length != RequiredSlotCount)
            {
                Debug.LogError($"報酬表示には{RequiredSlotCount}件の表示モデルが必要です。", this);
                Hide();
                return;
            }
            if (titleText != null) titleText.text = "戦闘報酬を選んでください";
            for (int i = 0; i < slots.Length; i++)
            {
                if (i < models.Length) slots[i].Render(models[i], NotifySelected);
                else slots[i].Hide();
            }
            panelRoot?.SetActive(true);
        }

        public void Hide() => panelRoot?.SetActive(false);

        private void NotifySelected(int choiceId) => RewardSelected?.Invoke(choiceId);

        public bool ValidateReferences()
        {
            if (panelRoot == null || titleText == null || slots == null || slots.Length != RequiredSlotCount)
                return false;
            foreach (RewardChoiceSlotView slot in slots)
                if (slot == null || !slot.ValidateReferences()) return false;
            return true;
        }
    }
}
