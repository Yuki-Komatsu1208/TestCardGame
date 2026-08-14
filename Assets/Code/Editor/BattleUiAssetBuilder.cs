using System;
using System.Collections.Generic;
using TMPro;
using TestCardGame.Cards.Views;
using TestCardGame.Controller;
using TestCardGame.UIs.Battle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TestCardGame.Editor
{
    /// <summary>PLAN04のBattle HUDと50枠手札をPrefabおよびSceneへ事前構築する。</summary>
    public static class BattleUiAssetBuilder
    {
        private const string ScenePath = "Assets/GameData/Scenes/BattleScene.unity";
        private const string PrefabDirectory = "Assets/GameData/Prefabs/UI/Battle";
        private const string HudPrefabPath = PrefabDirectory + "/BattleHud.prefab";
        private const string HandPrefabPath = PrefabDirectory + "/BattleHand.prefab";
        private const int EnemySlotCount = 12;

        private static readonly Color Iron = new(0.12f, 0.11f, 0.13f, 0.96f);
        private static readonly Color Parchment = new(0.82f, 0.75f, 0.58f, 1f);
        private static readonly Color ParchmentDark = new(0.58f, 0.48f, 0.34f, 1f);
        private static readonly Color Ink = new(0.11f, 0.09f, 0.07f, 1f);
        private static readonly Color Blood = new(0.62f, 0.10f, 0.10f, 1f);
        private static readonly Color Stun = new(0.82f, 0.60f, 0.12f, 1f);

        [MenuItem("Tools/Test Card Game/UI/Build Battle HUD And Hand")]
        public static void BuildAll()
        {
            EnsureDirectory(PrefabDirectory);
            BuildHudPrefab();
            BuildHandPrefab();
            InstallIntoScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("PLAN04: Battle HUDと50枠手札の事前構築が完了しました。");
        }

        /// <summary>敵HUD枠数の回答を待たずに、50枠手札Prefabだけを生成する。</summary>
        [MenuItem("Tools/Test Card Game/UI/Build Battle Hand Only")]
        public static void BuildHandOnly()
        {
            EnsureDirectory(PrefabDirectory);
            BuildHandPrefab();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("PLAN04: Battle用50枠手札Prefabの事前構築が完了しました。");
        }

        /// <summary>バッチモードからPrefab/Scene生成と検証を連続実行する。</summary>
        public static void BuildAndValidateBatch()
        {
            BuildAll();
            BattleUiAssetValidator.Validate();
        }

        /// <summary>12枠の敵HUDを含む常設HUD Prefabを作成する。</summary>
        private static void BuildHudPrefab()
        {
            var root = CreateImage("BattleHudRoot", null, new Vector2(0.02f, 0.58f), new Vector2(0.98f, 0.76f),
                Vector2.zero, Vector2.zero, Iron, false);
            var view = root.AddComponent<BattleHudView>();
            root.AddComponent<BattleConsoleLogForwarder>();

            var progress = CreateImage("Progress", root.transform, new Vector2(0f, 0.72f), new Vector2(1f, 1f),
                Vector2.zero, Vector2.zero, Parchment, false);
            TMP_Text turn = CreateText("Turn", progress.transform, "TURN 1  PLAYER", 25f, FontStyles.Bold,
                TextAlignmentOptions.Left, new Vector2(0.015f, 0f), new Vector2(0.19f, 1f), Vector2.zero, Vector2.zero, Ink);
            TMP_Text expedition = CreateText("Expedition", progress.transform, "灰の遠征 / 戦闘 1", 23f, FontStyles.Bold,
                TextAlignmentOptions.Left, new Vector2(0.20f, 0f), new Vector2(0.45f, 1f), Vector2.zero, Vector2.zero, Ink);
            TMP_Text stage = CreateText("Stage", progress.transform, "ステージ名", 23f, FontStyles.Bold,
                TextAlignmentOptions.Left, new Vector2(0.46f, 0f), new Vector2(0.68f, 1f), Vector2.zero, Vector2.zero, Ink);
            TMP_Text overHunt = CreateText("OverHunt", progress.transform, "通常遠征", 23f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(0.69f, 0f), new Vector2(0.82f, 1f), Vector2.zero, Vector2.zero, Ink);
            TMP_Text pending = CreateText("PendingGold", progress.transform, "保留ゴールド 0 G", 22f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(0.82f, 0f), new Vector2(0.93f, 1f), Vector2.zero, Vector2.zero, Ink);
            Button deck = CreateButton("CheckDeckButton", progress.transform, "デッキ", new Vector2(0.935f, 0.10f),
                new Vector2(0.995f, 0.90f));

            UnitHudSlotView player = CreateUnitSlot("PlayerHud", root.transform, new Vector2(0f, 0f), new Vector2(0.28f, 0.68f), true);
            var enemiesPanel = CreateImage("Enemies", root.transform, new Vector2(0.29f, 0f), new Vector2(0.90f, 0.68f),
                Vector2.zero, Vector2.zero, new Color(0.18f, 0.16f, 0.17f, 1f), false);
            ScrollRect enemyScroll = CreateHorizontalScroll(enemiesPanel.transform, out RectTransform enemyContent);
            var enemies = new List<UnitHudSlotView>();
            for (int i = 0; i < EnemySlotCount; i++)
            {
                var slot = CreateUnitSlot($"EnemyHudSlot_{i + 1:00}", enemyContent, Vector2.zero, Vector2.zero, false);
                var rect = slot.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.anchoredPosition = new Vector2(i * 252f, 0f);
                rect.sizeDelta = new Vector2(242f, 112f);
                slot.gameObject.SetActive(i < 3);
                enemies.Add(slot);
            }
            enemyContent.sizeDelta = new Vector2(4f * 252f, 0f);
            enemyScroll.horizontal = false;
            enemyScroll.horizontalNormalizedPosition = 0f;

            Button endTurn = CreateButton("EndTurnButton", root.transform, "ターン終了", new Vector2(0.91f, 0.08f),
                new Vector2(0.995f, 0.63f));
            SetReference(view, "playerSlot", player);
            SetArray(view, "enemySlots", enemies.ToArray());
            SetReference(view, "enemyScrollRect", enemyScroll);
            SetReference(view, "enemyContent", enemyContent);
            SetReference(view, "turnText", turn);
            SetReference(view, "expeditionText", expedition);
            SetReference(view, "stageText", stage);
            SetReference(view, "overHuntText", overHunt);
            SetReference(view, "pendingGoldText", pending);
            SetReference(view, "endTurnButton", endTurn);
            SetReference(view, "deckButton", deck);
            PrefabUtility.SaveAsPrefabAsset(root, HudPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        /// <summary>ScrollRectと50個のカード枠を持つBattle手札Prefabを作成する。</summary>
        private static void BuildHandPrefab()
        {
            var root = CreateImage("BattleHandRoot", null, new Vector2(0.02f, 0.01f), new Vector2(0.98f, 0.27f),
                Vector2.zero, Vector2.zero, Iron, false);
            var hand = root.AddComponent<HandView>();
            ScrollRect scroll = CreateHorizontalScroll(root.transform, out RectTransform content);
            RectTransform viewport = scroll.viewport;
            var slots = new List<BattleHandCardSlotView>();
            for (int i = 0; i < HandView.RequiredSlotCount; i++)
            {
                var slot = CreateCardSlot($"HandCardSlot_{i + 1:00}", content);
                slot.gameObject.SetActive(i < 8);
                slots.Add(slot);
            }
            SetReference(hand, "scrollRect", scroll);
            SetReference(hand, "viewport", viewport);
            SetReference(hand, "content", content);
            SetArray(hand, "slots", slots.ToArray());
            GameObject selectionHeader = CreateImage("CombatSelectionHeader", root.transform,
                new Vector2(0.20f, 0.80f), new Vector2(0.80f, 1f), Vector2.zero, Vector2.zero,
                Parchment, true);
            TMP_Text selectionTitle = CreateText("Title", selectionHeader.transform, "対象カードを選択", 20f,
                FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.02f, 0f), new Vector2(0.54f, 1f),
                Vector2.zero, Vector2.zero, Ink);
            TMP_Text selectionCount = CreateText("SelectionCount", selectionHeader.transform, "0 / 1", 20f,
                FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.55f, 0f), new Vector2(0.67f, 1f),
                Vector2.zero, Vector2.zero, Ink);
            Button selectionOk = CreateButton("OK", selectionHeader.transform, "OK",
                new Vector2(0.68f, 0.12f), new Vector2(0.83f, 0.88f));
            Button selectionCancel = CreateButton("Cancel", selectionHeader.transform, "NO",
                new Vector2(0.84f, 0.12f), new Vector2(0.98f, 0.88f));
            SetReference(hand, "selectionHeaderRoot", selectionHeader);
            SetReference(hand, "selectionTitleText", selectionTitle);
            SetReference(hand, "selectionCountText", selectionCount);
            SetReference(hand, "selectionOkButton", selectionOk);
            SetReference(hand, "selectionCancelButton", selectionCancel);
            selectionHeader.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, HandPrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        /// <summary>PrefabをBattleSceneへ配置し、既存ControllerのInspector参照を接続する。</summary>
        private static void InstallIntoScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = FindSceneCanvas(scene);
            var gameController = FindSceneComponent<GameController>(scene);
            var battleUi = FindSceneComponent<BattleUI>(scene);
            var runController = FindSceneComponent<RunController>(scene);
            if (canvas == null)
                throw new InvalidOperationException("BattleScene: CanvasScalerを持つ基準Canvasがありません。");
            if (gameController == null || battleUi == null || runController == null)
                throw new InvalidOperationException("BattleScene: Canvasまたは必須Controllerがありません。");

            DestroySceneViews<BattleHudView>(canvas.transform);
            DestroySceneViews<HandView>(canvas.transform);
            DestroyChild(canvas.transform, "BattleHudRoot");
            DestroyChild(canvas.transform, "BattleHandRoot");
            DestroyChild(canvas.transform, "HandPanel");
            GameObject hudObject = InstantiatePrefab(HudPrefabPath, "BattleHudRoot", scene, canvas.transform);
            GameObject handObject = InstantiatePrefab(HandPrefabPath, "BattleHandRoot", scene, canvas.transform);
            hudObject.transform.SetSiblingIndex(Mathf.Max(0, canvas.transform.childCount - 2));
            handObject.transform.SetSiblingIndex(Mathf.Max(0, canvas.transform.childCount - 2));

            var hud = hudObject.GetComponent<BattleHudView>();
            var hand = handObject.GetComponent<HandView>();
            Button endTurn = hudObject.transform.Find("EndTurnButton").GetComponent<Button>();
            Button deck = hudObject.transform.Find("Progress/CheckDeckButton").GetComponent<Button>();
            SetReference(gameController, "handView", hand);
            SetReference(battleUi, "battleHudView", hud);
            SetReference(battleUi, "endTurnButton", endTurn);
            SetReference(battleUi, "checkDeckButton", deck);
            SetReference(runController, "handView", hand);

            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>対象Scene内からCanvasScalerを持つ基準Canvasだけを取得する。</summary>
        private static Canvas FindSceneCanvas(Scene scene)
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

        /// <summary>Editor内部オブジェクトを除外し、対象Scene階層からComponentを取得する。</summary>
        private static T FindSceneComponent<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
        }

        private static UnitHudSlotView CreateUnitSlot(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, bool player)
        {
            var root = CreateImage(name, parent, anchorMin, anchorMax, new Vector2(4f, 4f), new Vector2(-4f, -4f),
                Parchment, false);
            var canvasGroup = root.AddComponent<CanvasGroup>();
            var view = root.AddComponent<UnitHudSlotView>();
            TMP_Text title = CreateText("Name", root.transform, player ? "傭兵" : "敵", 23f, FontStyles.Bold,
                TextAlignmentOptions.Left, new Vector2(0.03f, 0.70f), new Vector2(0.38f, 0.98f), Vector2.zero, Vector2.zero, Ink);
            TMP_Text mana = CreateText("Mana", root.transform, player ? "マナ 5 / 5" : string.Empty, 18f, FontStyles.Bold,
                TextAlignmentOptions.Right, new Vector2(0.39f, 0.72f), new Vector2(0.97f, 0.98f), Vector2.zero, Vector2.zero, Ink);
            CreateGauge(root.transform, "Hp", "HP 100 / 100", Blood, 0.46f, out TMP_Text hp, out Image hpFill);
            CreateGauge(root.transform, "Stun", "スタン 100 / 100", Stun, 0.23f, out TMP_Text stun, out Image stunFill);
            TMP_Text status = CreateText("Statuses", root.transform, "状態異常: なし", 16f, FontStyles.Normal,
                TextAlignmentOptions.Left, new Vector2(0.03f, 0.01f), new Vector2(0.97f, 0.22f), Vector2.zero, Vector2.zero, Ink);
            SetReference(view, "nameText", title);
            SetReference(view, "hpText", hp);
            SetReference(view, "hpFill", hpFill);
            SetReference(view, "stunText", stun);
            SetReference(view, "stunFill", stunFill);
            SetReference(view, "manaText", mana);
            SetReference(view, "statusText", status);
            SetReference(view, "canvasGroup", canvasGroup);
            return view;
        }

        private static BattleHandCardSlotView CreateCardSlot(string name, Transform parent)
        {
            var root = CreateImage(name, parent, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, ParchmentDark, true);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(150f, 210f);
            var group = root.AddComponent<CanvasGroup>();
            var card = root.AddComponent<CardView>();
            root.AddComponent<MoveCardDragHandler>();
            var slot = root.AddComponent<BattleHandCardSlotView>();
            TMP_Text title = CreateText("Name", root.transform, "カード名 (Lv.1)", 19f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(0.04f, 0.77f), new Vector2(0.96f, 0.98f), Vector2.zero, Vector2.zero, Ink);
            TMP_Text cost = CreateText("Cost", root.transform, "M:1  CT:0", 17f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(0.04f, 0.65f), new Vector2(0.96f, 0.77f), Vector2.zero, Vector2.zero, Ink);
            TMP_Text description = CreateText("Description", root.transform, "カード効果の説明", 15f, FontStyles.Normal,
                TextAlignmentOptions.TopLeft, new Vector2(0.07f, 0.06f), new Vector2(0.93f, 0.63f), Vector2.zero, Vector2.zero, Ink);
            SetReference(card, "nameText", title);
            SetReference(card, "costText", cost);
            SetReference(card, "descriptionText", description);
            SetReference(slot, "cardView", card);
            SetReference(slot, "canvasGroup", group);
            return slot;
        }

        private static void CreateGauge(Transform parent, string name, string label, Color fillColor, float minY,
            out TMP_Text text, out Image fill)
        {
            var root = CreateImage(name, parent, new Vector2(0.03f, minY), new Vector2(0.97f, minY + 0.20f),
                Vector2.zero, Vector2.zero, new Color(0.18f, 0.16f, 0.14f, 1f), false);
            GameObject fillObject = CreateImage("Fill", root.transform, Vector2.zero, Vector2.one, new Vector2(2f, 2f),
                new Vector2(-2f, -2f), fillColor, false);
            fill = fillObject.GetComponent<Image>();
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 1f;
            text = CreateText("Text", root.transform, label, 16f, FontStyles.Bold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        }

        private static ScrollRect CreateHorizontalScroll(Transform parent, out RectTransform content)
        {
            var root = CreateImage("Scroll", parent, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f),
                Color.clear, false);
            var scroll = root.AddComponent<ScrollRect>();
            GameObject viewportObject = CreateImage("Viewport", root.transform, Vector2.zero, Vector2.one, Vector2.zero,
                Vector2.zero, Color.clear, true);
            viewportObject.AddComponent<RectMask2D>();
            GameObject contentObject = CreateRectObject("Content", viewportObject.transform, new Vector2(0f, 0f),
                new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            content = contentObject.GetComponent<RectTransform>();
            content.pivot = new Vector2(0f, 0f);
            content.sizeDelta = new Vector2(1f, 0f);
            scroll.viewport = viewportObject.GetComponent<RectTransform>();
            scroll.content = content;
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 36f;
            return scroll;
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject root = CreateImage(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero, ParchmentDark, true);
            var button = root.AddComponent<Button>();
            CreateText("Text", root.transform, label, 20f, FontStyles.Bold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(4f, 2f), new Vector2(-4f, -2f), Color.white);
            return button;
        }

        private static GameObject InstantiatePrefab(string path, string sceneObjectName, Scene scene, Transform parent)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path)
                ?? throw new InvalidOperationException($"Prefabがありません: {path}");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.transform.SetParent(parent, false);
            instance.name = sceneObjectName;
            return instance;
        }

        /// <summary>前回生成物を名前変更の有無にかかわらずComponent単位で安全に除去する。</summary>
        private static void DestroySceneViews<T>(Transform canvasRoot) where T : Component
        {
            T[] views = canvasRoot.GetComponentsInChildren<T>(true);
            foreach (T view in views)
            {
                if (view != null && view.gameObject != canvasRoot.gameObject)
                    UnityEngine.Object.DestroyImmediate(view.gameObject);
            }
        }

        private static void DestroyChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        private static GameObject CreateRectObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var root = new GameObject(name, typeof(RectTransform));
            if (parent != null) root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
            return root;
        }

        private static GameObject CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax, Color color, bool raycast)
        {
            GameObject root = CreateRectObject(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var image = root.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return root;
        }

        private static TMP_Text CreateText(string name, Transform parent, string label, float size, FontStyles style,
            TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            GameObject root = CreateRectObject(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            var text = root.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            return text;
        }

        private static void SetReference(UnityEngine.Object target, string name, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(name)
                ?? throw new InvalidOperationException($"{target.name}: {name} がありません。");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray<T>(UnityEngine.Object target, string name, T[] values) where T : UnityEngine.Object
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(name)
                ?? throw new InvalidOperationException($"{target.name}: {name} がありません。");
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureDirectory(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
