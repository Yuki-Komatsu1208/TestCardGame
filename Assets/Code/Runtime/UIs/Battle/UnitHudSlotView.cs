using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs.Battle
{
    /// <summary>ユニット1体分のHP、スタン、状態異常を専用欄へ描画する。</summary>
    public sealed class UnitHudSlotView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private Image hpFill;
        [SerializeField] private TMP_Text stunText;
        [SerializeField] private Image stunFill;
        [SerializeField] private TMP_Text manaText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private CanvasGroup canvasGroup;

        /// <summary>表示モデルを既存UI部品へ反映する。</summary>
        public void Render(UnitHudViewModel model)
        {
            if (model == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            if (nameText != null) nameText.text = model.IsDefeated ? $"{model.Name}（撃破済み）" : model.Name;
            if (hpText != null) hpText.text = $"HP {model.CurrentHp} / {model.MaxHp}";
            if (hpFill != null) hpFill.fillAmount = Mathf.Clamp01(model.HpRatio);
            if (stunText != null) stunText.text = $"スタン {model.CurrentStun} / {model.MaxStun}";
            if (stunFill != null) stunFill.fillAmount = Mathf.Clamp01(model.StunRatio);
            if (manaText != null)
            {
                manaText.gameObject.SetActive(model.MaxMana > 0);
                manaText.text = $"マナ {model.CurrentMana} / {model.MaxMana}";
            }
            if (statusText != null) statusText.text = BuildStatusText(model);
            if (canvasGroup != null) canvasGroup.alpha = model.IsDefeated ? 0.5f : 1f;
        }

        private static string BuildStatusText(UnitHudViewModel model)
        {
            if (model.Statuses == null || model.Statuses.Count == 0) return "状態異常: なし";
            var builder = new StringBuilder("状態異常: ");
            for (int i = 0; i < model.Statuses.Count; i++)
            {
                if (i > 0) builder.Append(" / ");
                builder.Append(model.Statuses[i].Label);
            }
            return builder.ToString();
        }

        /// <summary>必須のInspector参照が揃っているか返す。</summary>
        public bool HasRequiredReferences()
        {
            return nameText != null && hpText != null && hpFill != null && stunText != null && stunFill != null && statusText != null;
        }
    }
}
