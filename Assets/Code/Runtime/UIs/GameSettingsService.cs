using System;
using System.Collections.Generic;
using UnityEngine;

namespace TestCardGame.UIs
{
    /// <summary>プレイヤー設定を即時適用し、PlayerPrefsへ永続化するアプリケーションサービス。</summary>
    public static class GameSettingsService
    {
        private const string MasterVolumeKey = "Settings.Audio.Master";
        private const string BgmVolumeKey = "Settings.Audio.Bgm";
        private const string SeVolumeKey = "Settings.Audio.Se";
        private const string ResolutionWidthKey = "Settings.Screen.Width";
        private const string ResolutionHeightKey = "Settings.Screen.Height";
        private const string FullScreenKey = "Settings.Screen.FullScreen";
        private const string VSyncKey = "Settings.Screen.VSync";

        private static bool initialized;
        private static bool hasApplied;
        private static GameSettingsSnapshot current;

        public static event Action<GameSettingsSnapshot> Changed;
        public static GameSettingsSnapshot Current
        {
            get
            {
                EnsureInitialized();
                return current;
            }
        }

        /// <summary>保存済み設定を読み込み、現在の実行環境へ反映する。</summary>
        public static void EnsureInitialized()
        {
            if (initialized)
                return;

            int width = Mathf.Max(1, PlayerPrefs.GetInt(ResolutionWidthKey, Screen.width));
            int height = Mathf.Max(1, PlayerPrefs.GetInt(ResolutionHeightKey, Screen.height));
            current = new GameSettingsSnapshot(
                Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f)),
                Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumeKey, 1f)),
                Mathf.Clamp01(PlayerPrefs.GetFloat(SeVolumeKey, 1f)),
                width,
                height,
                PlayerPrefs.GetInt(FullScreenKey, Screen.fullScreen ? 1 : 0) != 0,
                PlayerPrefs.GetInt(VSyncKey, QualitySettings.vSyncCount > 0 ? 1 : 0) != 0);
            initialized = true;
            Apply(current, false);
        }

        public static void SetMasterVolume(float value)
        {
            EnsureInitialized();
            Update(Mathf.Clamp01(value), current.BgmVolume, current.SeVolume, current.ResolutionWidth,
                current.ResolutionHeight, current.IsFullScreen, current.IsVSyncEnabled);
        }

        public static void SetBgmVolume(float value)
        {
            EnsureInitialized();
            Update(current.MasterVolume, Mathf.Clamp01(value), current.SeVolume, current.ResolutionWidth,
                current.ResolutionHeight, current.IsFullScreen, current.IsVSyncEnabled);
        }

        public static void SetSeVolume(float value)
        {
            EnsureInitialized();
            Update(current.MasterVolume, current.BgmVolume, Mathf.Clamp01(value), current.ResolutionWidth,
                current.ResolutionHeight, current.IsFullScreen, current.IsVSyncEnabled);
        }

        public static void SetResolution(int width, int height)
        {
            EnsureInitialized();
            if (width <= 0 || height <= 0)
                return;
            Update(current.MasterVolume, current.BgmVolume, current.SeVolume, width, height,
                current.IsFullScreen, current.IsVSyncEnabled);
        }

        public static void SetFullScreen(bool enabled)
        {
            EnsureInitialized();
            Update(current.MasterVolume, current.BgmVolume, current.SeVolume, current.ResolutionWidth,
                current.ResolutionHeight, enabled, current.IsVSyncEnabled);
        }

        public static void SetVSync(bool enabled)
        {
            EnsureInitialized();
            Update(current.MasterVolume, current.BgmVolume, current.SeVolume, current.ResolutionWidth,
                current.ResolutionHeight, current.IsFullScreen, enabled);
        }

        /// <summary>実行環境が提示する解像度を幅と高さで重複除去して返す。</summary>
        public static IReadOnlyList<DisplayResolutionOption> GetAvailableResolutions()
        {
            var options = new List<DisplayResolutionOption>();
            var seen = new HashSet<(int width, int height)>();
            foreach (Resolution resolution in Screen.resolutions)
            {
                var key = (resolution.width, resolution.height);
                if (resolution.width <= 0 || resolution.height <= 0 || !seen.Add(key))
                    continue;
                options.Add(new DisplayResolutionOption(resolution.width, resolution.height));
            }

            if (seen.Add((Current.ResolutionWidth, Current.ResolutionHeight)))
                options.Add(new DisplayResolutionOption(Current.ResolutionWidth, Current.ResolutionHeight));
            options.Sort((left, right) => left.Width != right.Width
                ? left.Width.CompareTo(right.Width)
                : left.Height.CompareTo(right.Height));
            return options;
        }

        /// <summary>BGM再生側が使用するマスター込みの実効音量を返す。</summary>
        public static float GetEffectiveBgmVolume() => Current.MasterVolume * Current.BgmVolume;

        /// <summary>SE再生側が使用するマスター込みの実効音量を返す。</summary>
        public static float GetEffectiveSeVolume() => Current.MasterVolume * Current.SeVolume;

        private static void Update(float master, float bgm, float se, int width, int height, bool fullScreen, bool vSync)
        {
            EnsureInitialized();
            var next = new GameSettingsSnapshot(master, bgm, se, width, height, fullScreen, vSync);
            Apply(next, true);
        }

        /// <summary>設定を実行環境へ反映し、必要なら永続化して購読者へ通知する。</summary>
        private static void Apply(GameSettingsSnapshot settings, bool save)
        {
            GameSettingsSnapshot previous = current;
            current = settings;
            AudioListener.volume = settings.MasterVolume;
            if (!hasApplied || previous.IsVSyncEnabled != settings.IsVSyncEnabled)
                QualitySettings.vSyncCount = settings.IsVSyncEnabled ? 1 : 0;
            if (!hasApplied || previous.ResolutionWidth != settings.ResolutionWidth
                || previous.ResolutionHeight != settings.ResolutionHeight
                || previous.IsFullScreen != settings.IsFullScreen)
            {
                Screen.SetResolution(settings.ResolutionWidth, settings.ResolutionHeight,
                    settings.IsFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            }
            hasApplied = true;

            if (save)
            {
                PlayerPrefs.SetFloat(MasterVolumeKey, settings.MasterVolume);
                PlayerPrefs.SetFloat(BgmVolumeKey, settings.BgmVolume);
                PlayerPrefs.SetFloat(SeVolumeKey, settings.SeVolume);
                PlayerPrefs.SetInt(ResolutionWidthKey, settings.ResolutionWidth);
                PlayerPrefs.SetInt(ResolutionHeightKey, settings.ResolutionHeight);
                PlayerPrefs.SetInt(FullScreenKey, settings.IsFullScreen ? 1 : 0);
                PlayerPrefs.SetInt(VSyncKey, settings.IsVSyncEnabled ? 1 : 0);
                PlayerPrefs.Save();
            }

            Changed?.Invoke(current);
        }
    }
}
