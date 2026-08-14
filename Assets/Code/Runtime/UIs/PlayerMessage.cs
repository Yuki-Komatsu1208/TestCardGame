using System;

namespace TestCardGame.UIs
{
    public enum PlayerMessageKind
    {
        Normal,
        Warning,
        FatalError
    }

    /// <summary>画面間で受け渡すプレイヤー向けメッセージの表示専用データ。</summary>
    public readonly struct PlayerMessage
    {
        public string Text { get; }
        public PlayerMessageKind Kind { get; }
        public long Sequence { get; }
        public DateTime CreatedAtUtc { get; }
        public string TargetSceneName { get; }

        public PlayerMessage(string text, PlayerMessageKind kind, long sequence, DateTime createdAtUtc, string targetSceneName = null)
        {
            Text = text ?? string.Empty;
            Kind = kind;
            Sequence = sequence;
            CreatedAtUtc = createdAtUtc;
            TargetSceneName = targetSceneName ?? string.Empty;
        }

        /// <summary>指定されたSceneでこの通知を表示できるか返す。</summary>
        public bool CanBeConsumedBy(string sceneName)
        {
            return string.IsNullOrEmpty(TargetSceneName)
                || string.Equals(TargetSceneName, sceneName, StringComparison.Ordinal);
        }
    }
}
