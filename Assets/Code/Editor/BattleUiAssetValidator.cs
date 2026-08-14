using System;
using TestCardGame.UIs.Battle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TestCardGame.Editor
{
    /// <summary>PLAN04のPrefabとBattleSceneの事前配置数・必須参照を検査する。</summary>
    public static class BattleUiAssetValidator
    {
        private const string ScenePath = "Assets/GameData/Scenes/BattleScene.unity";
        private const string HudPrefabPath = "Assets/GameData/Prefabs/UI/Battle/BattleHud.prefab";
        private const string HandPrefabPath = "Assets/GameData/Prefabs/UI/Battle/BattleHand.prefab";

        [MenuItem("Tools/Test Card Game/UI/Validate Battle HUD And Hand")]
        public static void Validate()
        {
            ValidateHandPrefab();
            ValidateHudPrefab();
            ValidateScene();
            ValidateLayoutBoundaries();
            Debug.Log("PLAN04検証: Battle HUDと50枠手札に問題はありません。");
        }

        private static void ValidateHandPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HandPrefabPath)
                ?? throw new InvalidOperationException("BattleHand.prefabがありません。");
            var hand = prefab.GetComponent<HandView>()
                ?? throw new InvalidOperationException("BattleHand.prefabにHandViewがありません。");
            if (hand.Slots.Count != HandView.RequiredSlotCount)
                throw new InvalidOperationException($"Battle手札は{HandView.RequiredSlotCount}枠必要です。");
            if (!hand.ValidateReferences())
                throw new InvalidOperationException("BattleHand.prefabの必須参照が不足しています。");
        }

        private static void ValidateHudPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath)
                ?? throw new InvalidOperationException("BattleHud.prefabがありません。");
            var hud = prefab.GetComponent<BattleHudView>()
                ?? throw new InvalidOperationException("BattleHud.prefabにBattleHudViewがありません。");
            if (prefab.GetComponent<BattleConsoleLogForwarder>() == null)
                throw new InvalidOperationException("BattleHud.prefabに戦闘ログ転送Componentがありません。");
            if (!hud.ValidateReferences())
                throw new InvalidOperationException("BattleHud.prefabの必須参照が不足しています。");
            if (hud.EnemySlots.Count != 12)
                throw new InvalidOperationException("敵HUDは確定仕様どおり12枠必要です。");
            RectTransform hudRect = prefab.GetComponent<RectTransform>();
            if (hudRect.anchorMax.y > 0.76f || hudRect.anchorMin.y < 0.58f)
                throw new InvalidOperationException("Battle HUDが共通上部領域の直下へ収まっていません。");
        }

        private static void ValidateScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = FindSceneCanvas(scene)
                ?? throw new InvalidOperationException("BattleSceneにCanvasがありません。");
            var scaler = canvas.GetComponent<CanvasScaler>()
                ?? throw new InvalidOperationException("BattleSceneのCanvasScalerがありません。");
            if (scaler.referenceResolution != new Vector2(1920f, 1080f))
                throw new InvalidOperationException("BattleSceneの基準解像度が1920x1080ではありません。");
            Transform hudRoot = canvas.transform.Find("BattleHudRoot");
            Transform handRoot = canvas.transform.Find("BattleHandRoot");
            if (hudRoot == null || handRoot == null)
                throw new InvalidOperationException("BattleSceneにBattle HUDまたは手札が事前配置されていません。");
            var hudViews = canvas.GetComponentsInChildren<BattleHudView>(true);
            var handViews = canvas.GetComponentsInChildren<HandView>(true);
            var logForwarders = canvas.GetComponentsInChildren<BattleConsoleLogForwarder>(true);
            if (hudViews.Length != 1 || handViews.Length != 1)
                throw new InvalidOperationException($"BattleSceneのUIが重複しています。HUD {hudViews.Length}個、手札 {handViews.Length}個です。");
            if (logForwarders.Length != 1)
                throw new InvalidOperationException($"BattleSceneの戦闘ログ転送Componentは1個必要です。現在{logForwarders.Length}個です。");
            if (hudViews[0].transform != hudRoot || handViews[0].transform != handRoot)
                throw new InvalidOperationException("BattleSceneのHUDまたは手札Componentが規定ルートにありません。");
        }

        /// <summary>対象Scene内からCanvasScalerを持つ基準Canvasだけを取得する。</summary>
        private static Canvas FindSceneCanvas(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
                {
                    if (canvas.GetComponent<CanvasScaler>() != null) return canvas;
                }
            }
            return null;
        }

        private static void ValidateLayoutBoundaries()
        {
            int[] counts = { 0, 1, 20, 21, 50 };
            foreach (int count in counts)
            {
                bool actual = HandView.ShouldUseScroll(count, 20);
                bool expected = count > 20;
                if (actual != expected)
                    throw new InvalidOperationException($"手札{count}枚のスクロール切替が不正です。");
                float step = HandView.CalculateCardStep(count, 1800f, 150f, 54f, 94f, 86f, 22f, actual);
                if (count > 1 && step < 54f)
                    throw new InvalidOperationException($"手札{count}枚でカード識別幅が不足します。");
                float contentWidth = 44f + 150f + step * Mathf.Max(0, count - 1);
                if (count == 21 && contentWidth <= 1800f)
                    throw new InvalidOperationException("手札21枚で横スクロール可能なContent幅になりません。");
            }
        }
    }
}
