#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TestCardGame.Runtime
{
    /// <summary>
    /// Editor の Play Mode 中だけ描画頻度を抑える。
    /// ビルドにはコンパイルされず、終了時には変更前の設定へ戻す。
    /// </summary>
    [InitializeOnLoad]
    public static class PerformanceSettings
    {
        private const int PlayModeVSyncCount = 1;
        private const int PlayModeRenderFrameInterval = 2;

        private static int previousVSyncCount;
        private static int previousRenderFrameInterval;
        private static bool hasCapturedSettings;

        static PerformanceSettings()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    ApplyPlayModeLimits();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    RestoreSettings();
                    break;
            }
        }

        private static void ApplyPlayModeLimits()
        {
            previousVSyncCount = QualitySettings.vSyncCount;
            previousRenderFrameInterval = OnDemandRendering.renderFrameInterval;
            hasCapturedSettings = true;

            QualitySettings.vSyncCount = PlayModeVSyncCount;
            OnDemandRendering.renderFrameInterval = PlayModeRenderFrameInterval;
        }

        private static void RestoreSettings()
        {
            if (!hasCapturedSettings)
            {
                return;
            }

            QualitySettings.vSyncCount = previousVSyncCount;
            OnDemandRendering.renderFrameInterval = previousRenderFrameInterval;
            hasCapturedSettings = false;
        }
    }
}
#endif
