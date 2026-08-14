using TMPro;
using UnityEngine;

namespace TestCardGame.UIs
{
    /// <summary>事前配置された欄へ画面固有のステータス表示データを反映するView。</summary>
    public sealed class CommonStatusBarView : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] labels = System.Array.Empty<TMP_Text>();
        [SerializeField] private TMP_Text[] values = System.Array.Empty<TMP_Text>();

        public int SlotCount => Mathf.Min(labels?.Length ?? 0, values?.Length ?? 0);

        /// <summary>渡された表示専用データを先頭から既存スロットへ反映する。</summary>
        public void Render(System.Collections.Generic.IReadOnlyList<CommonStatusItemModel> items)
        {
            int itemCount = items?.Count ?? 0;
            for (int i = 0; i < SlotCount; i++)
            {
                bool visible = i < itemCount;
                labels[i].gameObject.SetActive(visible);
                values[i].gameObject.SetActive(visible);
                if (!visible)
                    continue;

                labels[i].text = items[i].Label;
                values[i].text = items[i].Value;
            }
        }

        /// <summary>Inspector参照の不足を日本語エラーとして報告する。</summary>
        public bool ValidateReferences()
        {
            if (labels == null || values == null || labels.Length == 0 || labels.Length != values.Length)
            {
                Debug.LogError($"{name}: ステータス欄のラベルと値の参照数が一致していません。", this);
                return false;
            }

            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] != null && values[i] != null)
                    continue;

                Debug.LogError($"{name}: ステータス欄 {i} の参照が不足しています。", this);
                return false;
            }

            return true;
        }
    }
}
