using TestCardGame.UIs;
using UnityEngine;

namespace TestCardGame.UIs.Battle
{
    /// <summary>Battle中のConsole本文を共通上部ログへ転送し、スタックトレースはConsoleだけに残す。</summary>
    public sealed class BattleConsoleLogForwarder : MonoBehaviour
    {
        private bool isForwarding;

        private void OnEnable()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        /// <summary>ログ本文だけを非致命メッセージとして転送し、再帰的な通知を防止する。</summary>
        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (isForwarding || string.IsNullOrWhiteSpace(condition)) return;
            isForwarding = true;
            try
            {
                PlayerMessageKind kind = type == LogType.Log ? PlayerMessageKind.Normal : PlayerMessageKind.Warning;
                PlayerMessageService.PublishForScene("BattleScene", condition, kind);
            }
            finally
            {
                isForwarding = false;
            }
        }
    }
}
