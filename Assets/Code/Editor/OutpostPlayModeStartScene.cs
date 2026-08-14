#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TestCardGame.Editor
{
    /// <summary>エディタでの再生開始地点をOutpostに固定する。</summary>
    [InitializeOnLoad]
    internal static class OutpostPlayModeStartScene
    {
        private const string OutpostScenePath = "Assets/GameData/Scenes/OutpostScene.unity";

        static OutpostPlayModeStartScene()
        {
            EditorApplication.playModeStateChanged += SetOutpostAsPlayModeStartScene;
        }

        /// <summary>再生開始直前にOutpostSceneをPlay Mode開始シーンへ設定する。</summary>
        private static void SetOutpostAsPlayModeStartScene(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
            {
                return;
            }

            var outpostScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(OutpostScenePath);
            if (outpostScene == null)
            {
                Debug.LogError("OutpostPlayModeStartScene: OutpostScene を読み込めません。");
                return;
            }

            EditorSceneManager.playModeStartScene = outpostScene;
        }
    }
}
#endif
