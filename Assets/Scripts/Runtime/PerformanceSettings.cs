using UnityEngine;

namespace TestCardGame.Runtime
{
    public static class PerformanceSettings
    {
        private const int TargetFrameRate = 30;
        private const string TargetQualityName = "Low";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            ApplyQualityLevel();
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFrameRate;
        }

        /// <summary>
        /// 実行時の品質を低めに固定する。
        /// </summary>
        private static void ApplyQualityLevel()
        {
            string[] qualityNames = QualitySettings.names;
            for (int i = 0; i < qualityNames.Length; i++)
            {
                if (qualityNames[i] == TargetQualityName)
                {
                    QualitySettings.SetQualityLevel(i, true);
                    return;
                }
            }
        }
    }
}
