using System.Collections.Generic;
using UnityEngine;

namespace TestCardGame.UIs
{
    /// <summary>設定Viewからの変更要求を設定サービスへ渡し、適用後の値を再描画する。</summary>
    public sealed class SettingsController : MonoBehaviour
    {
        [SerializeField] private SettingsModalView view;

        private IReadOnlyList<DisplayResolutionOption> resolutions;

        private void Awake()
        {
            if (view == null)
            {
                Debug.LogError($"{name}: 設定Viewの参照がありません。", this);
                return;
            }

            GameSettingsService.EnsureInitialized();
            resolutions = GameSettingsService.GetAvailableResolutions();
            view.MasterVolumeChanged += GameSettingsService.SetMasterVolume;
            view.BgmVolumeChanged += GameSettingsService.SetBgmVolume;
            view.SeVolumeChanged += GameSettingsService.SetSeVolume;
            view.PreviousResolutionRequested += SelectPreviousResolution;
            view.NextResolutionRequested += SelectNextResolution;
            view.FullScreenChanged += GameSettingsService.SetFullScreen;
            view.VSyncChanged += GameSettingsService.SetVSync;
            GameSettingsService.Changed += Render;
            Render(GameSettingsService.Current);
        }

        private void OnDestroy()
        {
            if (view != null)
            {
                view.MasterVolumeChanged -= GameSettingsService.SetMasterVolume;
                view.BgmVolumeChanged -= GameSettingsService.SetBgmVolume;
                view.SeVolumeChanged -= GameSettingsService.SetSeVolume;
                view.PreviousResolutionRequested -= SelectPreviousResolution;
                view.NextResolutionRequested -= SelectNextResolution;
                view.FullScreenChanged -= GameSettingsService.SetFullScreen;
                view.VSyncChanged -= GameSettingsService.SetVSync;
            }
            GameSettingsService.Changed -= Render;
        }

        private void SelectPreviousResolution() => SelectResolution(-1);
        private void SelectNextResolution() => SelectResolution(1);

        /// <summary>現在値を基準に解像度候補を循環し、選択結果を即時適用する。</summary>
        private void SelectResolution(int direction)
        {
            if (resolutions == null || resolutions.Count == 0)
                return;

            GameSettingsSnapshot current = GameSettingsService.Current;
            int currentIndex = 0;
            for (int i = 0; i < resolutions.Count; i++)
            {
                if (resolutions[i].Width == current.ResolutionWidth && resolutions[i].Height == current.ResolutionHeight)
                {
                    currentIndex = i;
                    break;
                }
            }

            int nextIndex = (currentIndex + direction + resolutions.Count) % resolutions.Count;
            DisplayResolutionOption next = resolutions[nextIndex];
            GameSettingsService.SetResolution(next.Width, next.Height);
        }

        private void Render(GameSettingsSnapshot settings)
        {
            view?.Render(settings);
        }
    }
}
