using System;
using System.Linq;
using TestCardGame.BoardManage;
using TestCardGame.Controller;
using TestCardGame.UIs;
using TestCardGame.UIs.Battle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

namespace TestCardGame.Editor
{
    /// <summary>PLAN01〜07で事前配置したViewをControllerのInspector参照へ統合する。</summary>
    public static class UiIntegrationAssetBuilder
    {
        private const string BattleScenePath = "Assets/GameData/Scenes/BattleScene.unity";
        private const string CardListPrefabPath = "Assets/GameData/Prefabs/CardList/CardListSelectionOverlay.prefab";
        private const string HandPrefabPath = "Assets/GameData/Prefabs/UI/Battle/BattleHand.prefab";

        [MenuItem("Tools/Test Card Game/UI/Build And Validate UI Integration")]
        public static void BuildAndValidate()
        {
            EnsureHandPrefabSelectionControls();
            Scene scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
            GameController gameController = Require<GameController>(scene);
            RunController runController = Require<RunController>(scene);
            GameStarter gameStarter = Require<GameStarter>(scene);
            CellBuilder cellBuilder = Require<CellBuilder>(scene);
            HandView handView = Require<HandView>(scene);
            EnsureHandSelectionControls(handView);
            BoardView boardView = Require<BoardView>(scene);
            BattleUI battleUi = Require<BattleUI>(scene);
            RewardController rewardController = runController.GetComponent<RewardController>();
            if (rewardController == null)
                rewardController = runController.gameObject.AddComponent<RewardController>();
            CardListSelectionOverlay overlay = EnsureCardListOverlay(scene);
            TooltipView tooltip = Require<TooltipView>(scene);

            Assign(gameController, "cellBuilder", cellBuilder);
            Assign(gameController, "handView", handView);
            Assign(gameController, "boardView", boardView);
            Assign(gameController, "battleUIInstance", battleUi);
            Assign(battleUi, "cardListOverlay", overlay);
            Assign(runController, "gameController", gameController);
            Assign(runController, "handView", handView);
            Assign(runController, "rewardController", rewardController);
            Assign(rewardController, "gameController", gameController);
            Assign(gameStarter, "gameController", gameController);
            Assign(gameStarter, "handView", handView);
            Assign(gameStarter, "runController", runController);
            Assign(cellBuilder, "boardView", boardView);
            Assign(cellBuilder, "tooltipView", tooltip);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            ComprehensiveUiAssetValidator.ValidateAll();
            Debug.Log("PLAN08: UI統合参照の設定と総合検証が完了しました。");
        }

        /// <summary>BattleHand Prefabへ戦闘選択用の固定操作領域を追加する。</summary>
        private static void EnsureHandPrefabSelectionControls()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(HandPrefabPath);
            try
            {
                HandView hand = root.GetComponent<HandView>()
                    ?? throw new InvalidOperationException("BattleHand.prefabにHandViewがありません。");
                EnsureHandSelectionControls(hand);
                PrefabUtility.SaveAsPrefabAsset(root, HandPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>既存の手札構造を維持したまま、未配置時だけ選択数・OK・キャンセルUIを追加する。</summary>
        private static void EnsureHandSelectionControls(HandView hand)
        {
            SerializedObject serialized = new SerializedObject(hand);
            if (serialized.FindProperty("selectionHeaderRoot")?.objectReferenceValue != null
                && serialized.FindProperty("selectionTitleText")?.objectReferenceValue != null
                && serialized.FindProperty("selectionCountText")?.objectReferenceValue != null
                && serialized.FindProperty("selectionOkButton")?.objectReferenceValue != null
                && serialized.FindProperty("selectionCancelButton")?.objectReferenceValue != null)
                return;

            Transform old = hand.transform.Find("CombatSelectionHeader");
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            GameObject header = CreateImage("CombatSelectionHeader", hand.transform,
                new Vector2(0.20f, 0.80f), new Vector2(0.80f, 1f), new Color(0.82f, 0.75f, 0.58f, 1f));
            TMP_Text title = CreateText("Title", header.transform, "対象カードを選択", 20f,
                new Vector2(0.02f, 0f), new Vector2(0.54f, 1f));
            TMP_Text count = CreateText("SelectionCount", header.transform, "0 / 1", 20f,
                new Vector2(0.55f, 0f), new Vector2(0.67f, 1f));
            Button ok = CreateButton("OK", header.transform, "OK", new Vector2(0.68f, 0.12f), new Vector2(0.83f, 0.88f));
            Button cancel = CreateButton("Cancel", header.transform, "NO", new Vector2(0.84f, 0.12f), new Vector2(0.98f, 0.88f));
            Assign(hand, "selectionHeaderRoot", header);
            Assign(hand, "selectionTitleText", title);
            Assign(hand, "selectionCountText", count);
            Assign(hand, "selectionOkButton", ok);
            Assign(hand, "selectionCancelButton", cancel);
            header.SetActive(false);
        }

        private static GameObject CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = color;
            return root;
        }

        private static TMP_Text CreateText(string name, Transform parent, string value, float fontSize,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject root = CreateImage(name, parent, anchorMin, anchorMax, Color.clear);
            UnityEngine.Object.DestroyImmediate(root.GetComponent<Image>());
            var text = root.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.11f, 0.09f, 0.07f, 1f);
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string value, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject root = CreateImage(name, parent, anchorMin, anchorMax, new Color(0.58f, 0.48f, 0.34f, 1f));
            var button = root.AddComponent<Button>();
            CreateText("Text", root.transform, value, 20f, Vector2.zero, Vector2.one).color = Color.white;
            return button;
        }

        /// <summary>バッチ処理から利用する統合入口。</summary>
        public static void BuildAndValidateBatch()
        {
            try
            {
                BuildAndValidate();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static T Require<T>(Scene scene) where T : Component
        {
            T[] values = FindSceneComponents<T>(scene);
            if (values.Length != 1)
                throw new InvalidOperationException($"BattleSceneの{typeof(T).Name}は1件必要です。現在は{values.Length}件です。");
            return values[0];
        }

        /// <summary>旧カード一覧インスタンスを完成済み50枠Prefabへ置き換える。</summary>
        private static CardListSelectionOverlay EnsureCardListOverlay(Scene scene)
        {
            CardListSelectionOverlay[] current = FindSceneComponents<CardListSelectionOverlay>(scene);
            if (current.Length == 1)
            {
                SerializedProperty slots = new SerializedObject(current[0]).FindProperty("slots");
                if (slots != null && slots.arraySize == 50)
                    return current[0];
            }

            foreach (CardListSelectionOverlay overlay in current)
                UnityEngine.Object.DestroyImmediate(overlay.gameObject);

            Canvas canvas = FindSceneComponents<Canvas>(scene)
                .FirstOrDefault(value => value.GetComponent<UnityEngine.UI.CanvasScaler>() != null)
                ?? throw new InvalidOperationException("BattleSceneに基準Canvasがありません。");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CardListPrefabPath)
                ?? throw new InvalidOperationException("完成済みカード一覧Prefabがありません。");
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, canvas.transform) as GameObject;
            if (instance == null || !instance.TryGetComponent(out CardListSelectionOverlay result))
                throw new InvalidOperationException("カード一覧PrefabをBattleSceneへ配置できませんでした。");
            instance.name = "CardListSelectionOverlay";
            return result;
        }

        private static T[] FindSceneComponents<T>(Scene scene) where T : Component
        {
            var results = new System.Collections.Generic.List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
                results.AddRange(root.GetComponentsInChildren<T>(true));
            return results.ToArray();
        }

        private static void Assign(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException($"{target.GetType().Name}.{propertyName}がありません。");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
