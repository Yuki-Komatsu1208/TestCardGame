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
        private const int PlayModeVSyncCount = 0;
        private const int PlayModeTargetFrameRate = 30; // 発熱を優先する場合は30に変更する。
        private const int PlayModeRenderFrameInterval = 1;

        private static int previousVSyncCount;
        private static int previousTargetFrameRate;
        private static int previousRenderFrameInterval;
        private static bool hasCapturedSettings;

        static PerformanceSettings()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        // Play Modeへの出入りに合わせて制限を適用・解除する。
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

        // 元の設定を保存し、Play Modeを指定のフレームレートに制限する。
        private static void ApplyPlayModeLimits()
        {
            previousVSyncCount = QualitySettings.vSyncCount;
            previousTargetFrameRate = Application.targetFrameRate;
            previousRenderFrameInterval = OnDemandRendering.renderFrameInterval;
            hasCapturedSettings = true;

            QualitySettings.vSyncCount = PlayModeVSyncCount;
            Application.targetFrameRate = PlayModeTargetFrameRate;
            OnDemandRendering.renderFrameInterval = PlayModeRenderFrameInterval;
        }

        // Play Mode開始前の設定へ戻す。
        private static void RestoreSettings()
        {
            if (!hasCapturedSettings)
            {
                return;
            }

            QualitySettings.vSyncCount = previousVSyncCount;
            Application.targetFrameRate = previousTargetFrameRate;
            OnDemandRendering.renderFrameInterval = previousRenderFrameInterval;
            hasCapturedSettings = false;
        }
    }
}
#endif
