using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.UIs
{
    /// <summary>事前配置された設定モーダルの開閉と入力通知だけを担当するView。</summary>
    public sealed class SettingsModalView : MonoBehaviour
    {
        [SerializeField] private GameObject modalRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TMP_Text masterVolumeValue;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private TMP_Text bgmVolumeValue;
        [SerializeField] private Slider seVolumeSlider;
        [SerializeField] private TMP_Text seVolumeValue;
        [SerializeField] private Button previousResolutionButton;
        [SerializeField] private TMP_Text resolutionValue;
        [SerializeField] private Button nextResolutionButton;
        [SerializeField] private Toggle fullScreenToggle;
        [SerializeField] private Toggle vSyncToggle;

        private bool suppressNotifications;

        public event Action Closed;
        public event Action<float> MasterVolumeChanged;
        public event Action<float> BgmVolumeChanged;
        public event Action<float> SeVolumeChanged;
        public event Action PreviousResolutionRequested;
        public event Action NextResolutionRequested;
        public event Action<bool> FullScreenChanged;
        public event Action<bool> VSyncChanged;
        public bool IsOpen => modalRoot != null && modalRoot.activeSelf;

        private void Awake()
        {
            if (!ValidateReferences())
                return;
            closeButton.onClick.AddListener(Hide);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
            seVolumeSlider.onValueChanged.AddListener(OnSeVolumeChanged);
            previousResolutionButton.onClick.AddListener(OnPreviousResolutionRequested);
            nextResolutionButton.onClick.AddListener(OnNextResolutionRequested);
            fullScreenToggle.onValueChanged.AddListener(OnFullScreenChanged);
            vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
        }

        private void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Hide);
            masterVolumeSlider?.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            bgmVolumeSlider?.onValueChanged.RemoveListener(OnBgmVolumeChanged);
            seVolumeSlider?.onValueChanged.RemoveListener(OnSeVolumeChanged);
            previousResolutionButton?.onClick.RemoveListener(OnPreviousResolutionRequested);
            nextResolutionButton?.onClick.RemoveListener(OnNextResolutionRequested);
            fullScreenToggle?.onValueChanged.RemoveListener(OnFullScreenChanged);
            vSyncToggle?.onValueChanged.RemoveListener(OnVSyncChanged);
        }

        public void Show()
        {
            if (modalRoot != null)
                modalRoot.SetActive(true);
        }

        public void Hide()
        {
            if (modalRoot != null)
                modalRoot.SetActive(false);
            Closed?.Invoke();
        }

        /// <summary>適用済み設定を既存の各操作部品へ反映する。</summary>
        public void Render(GameSettingsSnapshot settings)
        {
            if (!ValidateReferences())
                return;

            suppressNotifications = true;
            masterVolumeSlider.SetValueWithoutNotify(settings.MasterVolume);
            bgmVolumeSlider.SetValueWithoutNotify(settings.BgmVolume);
            seVolumeSlider.SetValueWithoutNotify(settings.SeVolume);
            fullScreenToggle.SetIsOnWithoutNotify(settings.IsFullScreen);
            vSyncToggle.SetIsOnWithoutNotify(settings.IsVSyncEnabled);
            masterVolumeValue.text = $"{Mathf.RoundToInt(settings.MasterVolume * 100f)}%";
            bgmVolumeValue.text = $"{Mathf.RoundToInt(settings.BgmVolume * 100f)}%";
            seVolumeValue.text = $"{Mathf.RoundToInt(settings.SeVolume * 100f)}%";
            resolutionValue.text = $"{settings.ResolutionWidth} × {settings.ResolutionHeight}";
            suppressNotifications = false;
        }

        private void OnMasterVolumeChanged(float value)
        {
            if (!suppressNotifications) MasterVolumeChanged?.Invoke(value);
        }

        private void OnBgmVolumeChanged(float value)
        {
            if (!suppressNotifications) BgmVolumeChanged?.Invoke(value);
        }

        private void OnSeVolumeChanged(float value)
        {
            if (!suppressNotifications) SeVolumeChanged?.Invoke(value);
        }

        private void OnPreviousResolutionRequested() => PreviousResolutionRequested?.Invoke();
        private void OnNextResolutionRequested() => NextResolutionRequested?.Invoke();

        private void OnFullScreenChanged(bool enabled)
        {
            if (!suppressNotifications) FullScreenChanged?.Invoke(enabled);
        }

        private void OnVSyncChanged(bool enabled)
        {
            if (!suppressNotifications) VSyncChanged?.Invoke(enabled);
        }

        /// <summary>Inspector参照の不足を日本語エラーとして報告する。</summary>
        public bool ValidateReferences()
        {
            if (modalRoot != null && closeButton != null
                && masterVolumeSlider != null && masterVolumeValue != null
                && bgmVolumeSlider != null && bgmVolumeValue != null
                && seVolumeSlider != null && seVolumeValue != null
                && previousResolutionButton != null && resolutionValue != null && nextResolutionButton != null
                && fullScreenToggle != null && vSyncToggle != null)
                return true;
            Debug.LogError($"{name}: 設定モーダルの参照が不足しています。", this);
            return false;
        }
    }
}
