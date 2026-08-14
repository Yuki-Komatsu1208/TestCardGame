using System;
using TestCardGame.UIs;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TestCardGame.Editor
{
    /// <summary>共通UI PrefabとSceneの必須構成をEditModeで検査する。</summary>
    public static class CommonUiAssetValidator
    {
        private const string CommonUiPrefabPath = "Assets/GameData/Prefabs/CommonUI/CommonUiRoot.prefab";
        private static readonly string[] ScenePaths =
        {
            "Assets/GameData/Scenes/OutpostScene.unity",
            "Assets/GameData/Scenes/BattleScene.unity"
        };

        [MenuItem("TestCardGame/UI/共通UIアセットを検証")]
        public static void ValidateAll()
        {
            ValidateMessageQueueContract();
            ValidatePrefab();
            foreach (string scenePath in ScenePaths)
                ValidateScene(scenePath);
            Debug.Log("共通UIアセットの検証に成功しました。");
        }

        /// <summary>通知が発行順に一度だけ消費される契約を純C#範囲で検査する。</summary>
        private static void ValidateMessageQueueContract()
        {
            PlayerMessageService.Clear();
            PlayerMessageService.Publish("通常", PlayerMessageKind.Normal);
            PlayerMessageService.PublishForScene("OutpostScene", "警告", PlayerMessageKind.Warning);
            if (!PlayerMessageService.TryConsume("BattleScene", out PlayerMessage first) || first.Text != "通常"
                || PlayerMessageService.TryConsume("BattleScene", out _)
                || !PlayerMessageService.TryConsume("OutpostScene", out PlayerMessage second) || second.Text != "警告"
                || PlayerMessageService.TryConsume("OutpostScene", out _))
            {
                PlayerMessageService.Clear();
                throw new InvalidOperationException("プレイヤー向け通知の一度だけ消費する契約に違反しています。");
            }
            PlayerMessageService.Clear();
        }

        /// <summary>バッチ実行に失敗終了コードを返せる検証入口。</summary>
        public static void ValidateAllBatch()
        {
            try
            {
                ValidateAll();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidatePrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(CommonUiPrefabPath);
            try
            {
                var controller = root.GetComponent<CommonUiController>();
                if (controller == null || !controller.ValidateReferences())
                    throw new InvalidOperationException("CommonUiRoot Prefabの必須参照が不足しています。");
                if (controller.StatusBar.SlotCount != 8)
                    throw new InvalidOperationException("CommonUiRoot Prefabのステータス欄は8枠必要です。");
                if (controller.MessageLog.MaxVisibleMessages != 4)
                    throw new InvalidOperationException("CommonUiRoot Prefabのログ表示上限は4件必要です。");
                if (root.GetComponent<SettingsController>() == null)
                    throw new InvalidOperationException("CommonUiRoot Prefabに設定Controllerがありません。");
                if (root.transform.Find("SettingsModal") == null)
                    throw new InvalidOperationException("CommonUiRoot Prefabに設定モーダルがありません。");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void ValidateScene(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Canvas[] sceneCanvases = FindSceneComponents<Canvas>(scene);
            var canvases = new System.Collections.Generic.List<Canvas>();
            foreach (Canvas candidate in sceneCanvases)
            {
                if (candidate.GetComponent<CanvasScaler>() != null)
                    canvases.Add(candidate);
            }
            if (canvases.Count != 1)
                throw new InvalidOperationException($"{scenePath}: CanvasScalerを持つ基準Canvasは1つ必要です。現在は{canvases.Count}個です。");

            CanvasScaler scaler = canvases[0].GetComponent<CanvasScaler>();
            if (scaler == null || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize
                || scaler.referenceResolution != new Vector2(1920f, 1080f)
                || !Mathf.Approximately(scaler.matchWidthOrHeight, 0.5f))
                throw new InvalidOperationException($"{scenePath}: CanvasScalerが1920 x 1080基準になっていません。");

            CommonUiController[] commonUis = FindSceneComponents<CommonUiController>(scene);
            if (commonUis.Length != 1 || !commonUis[0].ValidateReferences())
                throw new InvalidOperationException($"{scenePath}: 共通UIは正しい参照を持つ1個だけ必要です。");
            if (commonUis[0].SettingsModal.IsOpen)
                throw new InvalidOperationException($"{scenePath}: 設定モーダルは初期非表示である必要があります。");

            EventSystem[] eventSystems = FindSceneComponents<EventSystem>(scene);
            if (eventSystems.Length != 1)
                throw new InvalidOperationException($"{scenePath}: EventSystemは1つ必要です。現在は{eventSystems.Length}個です。");
        }

        /// <summary>Editor内部オブジェクトを除外し、対象Sceneに属するComponentだけを取得する。</summary>
        private static T[] FindSceneComponents<T>(Scene scene) where T : Component
        {
            var results = new System.Collections.Generic.List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
                results.AddRange(root.GetComponentsInChildren<T>(true));
            return results.ToArray();
        }
    }
}
