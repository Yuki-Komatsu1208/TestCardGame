using System;
using TestCardGame.UIs.Battle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TestCardGame.Controller;

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
            foreach (var slot in prefab.GetComponentsInChildren<UnitHudSlotView>(true))
                if (!slot.HasRequiredReferences()) throw new InvalidOperationException("非表示の既存HUD枠の参照が失われています。");
            if (hud.EnemySlots.Count != 12)
                throw new InvalidOperationException("敵HUDは確定仕様どおり12枠必要です。");
            RectTransform hudRect = prefab.GetComponent<RectTransform>();
            if (!hud.IsCompactLayout || hudRect.anchorMin.y != 1f || hudRect.anchorMax.y != 1f
                || Mathf.Abs(hudRect.sizeDelta.y - 48f) > 0.01f)
                throw new InvalidOperationException("Battle HUDが48pxの上部帯になっていません。");
            ValidatePassiveGraphics(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameData/Prefabs/UI/Battle/UnitOverheadBars.prefab"));
            ValidatePassiveGraphics(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GameData/Prefabs/UI/Battle/UnitHoverDetails.prefab"));
            var bars = AssetDatabase.LoadAssetAtPath<UnitOverheadBarsView>("Assets/GameData/Prefabs/UI/Battle/UnitOverheadBars.prefab");
            if (bars == null || !bars.ValidateReferences()) throw new InvalidOperationException("頭上バーのSpriteまたはFilled設定が不正です。");
        }

        private static void ValidateScene()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try { ValidateScene(scene); }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }

        /// <summary>渡されたSceneを保存・置換せず、戦闘専用の表示経路を検証する。</summary>
        public static void ValidateScene(Scene scene)
        {
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
            BattleUI battleUi = FindSceneComponent<BattleUI>(scene);
            if (battleUi == null || !battleUi.ValidateReferences()) throw new InvalidOperationException("戦闘UIと頭上表示の参照が不足しています。");
            var module = FindSceneComponent<InputSystemUIInputModule>(scene);
            if (FindSceneComponent<EventSystem>(scene) == null || module == null || !module.enabled
                || module.point?.action == null || module.leftClick?.action == null || module.actionsAsset == null)
                throw new InvalidOperationException("EventSystemのInput System接続が不足しています。");
            var common = FindSceneComponent<TestCardGame.UIs.CommonUiController>(scene);
            if (common == null || !common.ValidateReferences() || common.StatusBar.gameObject.activeSelf || common.MessageLog.MaxVisibleMessages != 1)
                throw new InvalidOperationException("共通UIの通知または重複ステータス表示が不正です。");
            if (hudRoot.Find("PlayerHud").gameObject.activeSelf || hudRoot.Find("Enemies").gameObject.activeSelf)
                throw new InvalidOperationException("固定個体HUDが常設されています。");
            var overlay = canvas.transform.Find("BattleUnitOverlay");
            if (overlay == null || overlay.GetComponentsInChildren<UnitHoverDetailsView>(true).Length != 1)
                throw new InvalidOperationException("共有ホバーパネルは1枚必要です。");
            ValidatePassiveGraphics(overlay.gameObject);
            var board = FindSceneComponent<BoardView>(scene)
                ?? throw new InvalidOperationException("BattleSceneの盤面表示参照がありません。");
            var data = new SerializedObject(board);
            float maximum = data.FindProperty("maximumCellSize").floatValue;
            Vector4 padding = data.FindProperty("padding").vector4Value;
            foreach (var size in new[] {new Vector2Int(5,5),new Vector2Int(6,6),new Vector2Int(8,4),new Vector2Int(4,8)})
            {
                var result = BoardLayoutCalculator.Calculate(board.Viewport.rect.size, size, 5f, padding, maximum);
                if (result.ContentSize.x > board.Viewport.rect.width + 0.01f || result.ContentSize.y > board.Viewport.rect.height + 0.01f)
                    throw new InvalidOperationException($"盤面{size}が表示領域に収まりません。");
            }
        }

        /// <summary>バー・説明パネルの全Graphicが操作を遮らないことを確認する。</summary>
        private static void ValidatePassiveGraphics(GameObject root)
        {
            if (root == null) throw new InvalidOperationException("頭上表示のPrefabがありません。");
            foreach (var graphic in root.GetComponentsInChildren<Graphic>(true))
                if (graphic.raycastTarget) throw new InvalidOperationException($"{graphic.name}がポインター操作を遮ります。");
        }

        private static T FindSceneComponent<T>(Scene scene) where T : Component
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
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
