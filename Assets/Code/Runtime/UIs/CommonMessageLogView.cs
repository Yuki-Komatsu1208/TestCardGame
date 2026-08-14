using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace TestCardGame.UIs
{
    /// <summary>事前配置されたテキスト欄へプレイヤー向け通知履歴を描画するView。</summary>
    public sealed class CommonMessageLogView : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;
        [SerializeField, Min(1)] private int maxVisibleMessages = 5;
        [SerializeField] private Color normalColor = new(0.08f, 0.07f, 0.06f, 1f);
        [SerializeField] private Color warningColor = new(0.78f, 0.43f, 0.04f, 1f);
        [SerializeField] private Color fatalErrorColor = new(0.92f, 0.58f, 0.08f, 1f);

        private readonly List<PlayerMessage> history = new();

        public int MaxVisibleMessages => maxVisibleMessages;

        /// <summary>通知を履歴末尾へ追加し、最大表示件数を超えた古い行を除く。</summary>
        public void Append(PlayerMessage message)
        {
            history.Add(message);
            while (history.Count > maxVisibleMessages)
                history.RemoveAt(0);
            RenderHistory();
        }

        /// <summary>現在の画面内履歴を消去する。</summary>
        public void Clear()
        {
            history.Clear();
            if (messageText != null)
                messageText.text = string.Empty;
        }

        /// <summary>保持中の履歴を一つの事前配置テキストへ反映する。</summary>
        private void RenderHistory()
        {
            if (messageText == null)
            {
                Debug.LogError($"{name}: メッセージテキストの参照がありません。", this);
                return;
            }

            var builder = new StringBuilder();
            for (int i = 0; i < history.Count; i++)
            {
                if (i > 0)
                    builder.AppendLine();
                builder.Append("<color=#");
                builder.Append(ColorUtility.ToHtmlStringRGBA(ResolveColor(history[i].Kind)));
                builder.Append('>');
                builder.Append(history[i].Text);
                builder.Append("</color>");
            }

            messageText.text = builder.ToString();
            messageText.color = Color.white;
        }

        private Color ResolveColor(PlayerMessageKind kind)
        {
            return kind switch
            {
                PlayerMessageKind.Warning => warningColor,
                PlayerMessageKind.FatalError => fatalErrorColor,
                _ => normalColor
            };
        }

        /// <summary>Inspector参照の不足を日本語エラーとして報告する。</summary>
        public bool ValidateReferences()
        {
            if (messageText != null)
                return true;
            Debug.LogError($"{name}: メッセージテキストの参照がありません。", this);
            return false;
        }
    }
}
