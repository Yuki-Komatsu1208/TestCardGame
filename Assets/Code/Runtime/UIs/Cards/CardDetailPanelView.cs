using System.Text;
using TMPro;
using UnityEngine;

namespace TestCardGame.UIs.Cards
{
    /// <summary>現在フォーカス中カードの詳細を右側へまとめて表示する。</summary>
    public sealed class CardDetailPanelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text cooldownText;
        [SerializeField] private TMP_Text effectText;
        [SerializeField] private TMP_Text modifierListText;
        [SerializeField] private TMP_Text disabledReasonText;

        public void Show(CardDisplayModel model)
        {
            if (model == null)
            {
                Clear();
                return;
            }

            if (nameText != null) nameText.text = model.Name;
            if (levelText != null) levelText.text = $"レベル {model.CurrentLevel}";
            if (costText != null) costText.text = $"コスト {model.CurrentCost}";
            if (cooldownText != null) cooldownText.text = $"クールダウン {model.CurrentCooldown}";
            if (effectText != null) effectText.text = string.IsNullOrWhiteSpace(model.EffectDescription) ? "効果なし" : model.EffectDescription;
            if (modifierListText != null)
            {
                var builder = new StringBuilder();
                if (model.ModifierNames.Count == 0)
                    builder.Append("なし");
                else
                    foreach (string modifier in model.ModifierNames)
                        builder.Append("・").AppendLine(modifier);
                modifierListText.text = builder.ToString().TrimEnd();
            }
            if (disabledReasonText != null)
            {
                disabledReasonText.text = model.DisabledReason;
                disabledReasonText.gameObject.SetActive(!model.IsSelectable && !string.IsNullOrWhiteSpace(model.DisabledReason));
            }
        }

        public void Clear()
        {
            if (nameText != null) nameText.text = "カードを選択してください";
            if (levelText != null) levelText.text = string.Empty;
            if (costText != null) costText.text = string.Empty;
            if (cooldownText != null) cooldownText.text = string.Empty;
            if (effectText != null) effectText.text = string.Empty;
            if (modifierListText != null) modifierListText.text = "なし";
            if (disabledReasonText != null) disabledReasonText.gameObject.SetActive(false);
        }
    }
}
