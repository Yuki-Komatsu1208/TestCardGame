using System.Text;
using UnityEngine;

namespace TestCardGame.UIs.Battle
{
    /// <summary>全ユニットで共有する、画面内に収まる詳細説明欄。</summary>
    public sealed class UnitHoverDetailsView : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text detailsText;
        [SerializeField] private Vector2 offset = new Vector2(18f, -18f);
        private RectTransform rect;
        private readonly StringBuilder builder = new StringBuilder(256);
        public void Hide() => gameObject.SetActive(false);

        /// <summary>状態と予告を表示し、文字量に合わせた寸法でCanvas内へ制限する。</summary>
        public void Show(UnitHudViewModel model, string intent, Vector2 anchor, Rect bounds)
        {
            if (model == null || detailsText == null) { Hide(); return; }
            rect ??= GetComponent<RectTransform>();
            builder.Clear();
            builder.Append(model.Name).Append("\nHP ").Append(model.CurrentHp).Append(" / ").Append(model.MaxHp)
                .Append("\nスタン耐性 ").Append(model.CurrentStun).Append(" / ").Append(model.MaxStun);
            builder.Append("\n状態異常: ");
            if (model.Statuses.Count == 0) builder.Append("なし");
            for (int i = 0; i < model.Statuses.Count; i++)
            {
                if (i > 0) builder.Append(" / ");
                builder.Append(model.Statuses[i].Label);
            }
            if (!string.IsNullOrEmpty(intent)) builder.Append("\n次の行動: ").Append(intent);
            detailsText.text = builder.ToString();
            float width = Mathf.Max(1f, Mathf.Min(380f, bounds.width - 16f));
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            float height = Mathf.Clamp(detailsText.GetPreferredValues(detailsText.text, width - 24f, 0f).y + 24f,
                150f, Mathf.Max(150f, bounds.height - 16f));
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            Vector2 position = anchor + offset;
            position.x = Mathf.Clamp(position.x, bounds.xMin + 8f, Mathf.Max(bounds.xMin + 8f, bounds.xMax - width - 8f));
            position.y = Mathf.Clamp(position.y, bounds.yMin + height + 8f, bounds.yMax - 8f);
            rect.anchoredPosition = position;
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
        }

        public bool ValidateReferences() => detailsText != null && !detailsText.raycastTarget;
    }
}
