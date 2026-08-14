using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TestCardGame.UIs
{
    /// <summary>共通UIの入力と表示専用データの受け渡しを仲介する。</summary>
    public sealed class CommonUiController : MonoBehaviour
    {
        [SerializeField] private CommonStatusBarView statusBar;
        [SerializeField] private CommonMessageLogView messageLog;
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsModalView settingsModal;

        public CommonStatusBarView StatusBar => statusBar;
        public CommonMessageLogView MessageLog => messageLog;
        public SettingsModalView SettingsModal => settingsModal;

        private void Awake()
        {
            if (!ValidateReferences())
                return;
            settingsButton.onClick.AddListener(settingsModal.Show);
        }

        private void OnEnable()
        {
            PlayerMessageService.MessagePublished += ConsumePendingMessages;
            ConsumePendingMessages();
        }

        private void OnDisable()
        {
            PlayerMessageService.MessagePublished -= ConsumePendingMessages;
        }

        private void OnDestroy()
        {
            if (settingsButton != null && settingsModal != null)
                settingsButton.onClick.RemoveListener(settingsModal.Show);
        }

        public void RenderStatus(IReadOnlyList<CommonStatusItemModel> items)
        {
            statusBar?.Render(items);
        }

        /// <summary>待機中の通知をこの画面で一度だけ消費する。</summary>
        private void ConsumePendingMessages()
        {
            if (messageLog == null)
                return;
            string sceneName = SceneManager.GetActiveScene().name;
            while (PlayerMessageService.TryConsume(sceneName, out PlayerMessage message))
                messageLog.Append(message);
        }

        /// <summary>共通UI全体のInspector参照を検証する。</summary>
        public bool ValidateReferences()
        {
            bool valid = statusBar != null && messageLog != null && settingsButton != null && settingsModal != null;
            if (!valid)
            {
                Debug.LogError($"{name}: 共通UIのInspector参照が不足しています。", this);
                return false;
            }

            return statusBar.ValidateReferences() && messageLog.ValidateReferences() && settingsModal.ValidateReferences();
        }
    }
}
