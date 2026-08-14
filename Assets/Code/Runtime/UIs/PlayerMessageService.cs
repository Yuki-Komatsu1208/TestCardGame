using System;
using System.Collections.Generic;

namespace TestCardGame.UIs
{
    /// <summary>次に利用可能な画面が一度だけ消費する通知をセッション中保持する。</summary>
    public static class PlayerMessageService
    {
        private static readonly List<PlayerMessage> PendingMessages = new();
        private static long nextSequence;

        public static event Action MessagePublished;
        public static int PendingCount => PendingMessages.Count;

        /// <summary>プレイヤー向け通知を待機列へ追加する。</summary>
        public static void Publish(string text, PlayerMessageKind kind = PlayerMessageKind.Normal)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            PendingMessages.Add(new PlayerMessage(text, kind, ++nextSequence, DateTime.UtcNow));
            MessagePublished?.Invoke();
        }

        /// <summary>指定Sceneで表示する通知を待機列へ追加する。</summary>
        public static void PublishForScene(string sceneName, string text, PlayerMessageKind kind = PlayerMessageKind.Normal)
        {
            if (string.IsNullOrWhiteSpace(sceneName) || string.IsNullOrWhiteSpace(text))
                return;

            PendingMessages.Add(new PlayerMessage(text, kind, ++nextSequence, DateTime.UtcNow, sceneName));
            MessagePublished?.Invoke();
        }

        /// <summary>指定Sceneが消費できる最も古い通知を一度だけ取り出す。</summary>
        public static bool TryConsume(string sceneName, out PlayerMessage message)
        {
            for (int i = 0; i < PendingMessages.Count; i++)
            {
                if (!PendingMessages[i].CanBeConsumedBy(sceneName))
                    continue;

                message = PendingMessages[i];
                PendingMessages.RemoveAt(i);
                return true;
            }

            message = default;
            return false;
        }

        /// <summary>テストおよび新規セッション境界で待機通知を破棄する。</summary>
        public static void Clear()
        {
            PendingMessages.Clear();
            nextSequence = 0;
        }
    }
}
