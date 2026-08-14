using System;
using System.Collections.Generic;
using TestCardGame.Controller;
using TestCardGame.UIs.PostBattle;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TestCardGame.Editor
{
    /// <summary>PLAN07の戦闘後画面をPrefabとBattleSceneへ事前構築する。</summary>
    public static class PostBattleUiAssetBuilder
    {
        private const string ScenePath = "Assets/GameData/Scenes/BattleScene.unity";
        private const string PrefabDirectory = "Assets/GameData/Prefabs/UI/PostBattle";
        private const string PrefabPath = PrefabDirectory + "/PostBattleRoot.prefab";
        private static readonly Color Iron = new(0.10f, 0.09f, 0.11f, 0.97f);
        private static readonly Color Parchment = new(0.84f, 0.77f, 0.61f, 1f);
        private static readonly Color Ink = new(0.10f, 0.08f, 0.06f, 1f);
        private static readonly Color Gold = new(0.92f, 0.70f, 0.16f, 1f);
        private static readonly Color Green = new(0.25f, 0.62f, 0.29f, 1f);
        private static readonly Color Red = new(0.68f, 0.16f, 0.14f, 1f);

        [MenuItem("Tools/Test Card Game/UI/Build Post Battle And Recovery")]
        public static void BuildAll()
        {
            EnsureDirectory(PrefabDirectory);
            BuildPrefab();
            InstallIntoScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("PLAN07: 戦闘後・死亡・復旧UIの事前構築が完了しました。");
        }

        /// <summary>バッチ実行でPrefab/Scene構築後に必須参照を検証する。</summary>
        public static void BuildAndValidateBatch()
        {
            BuildAll();
            PostBattleUiAssetValidator.Validate();
        }

        private static void BuildPrefab()
        {
            GameObject root = CreateRect("PostBattleRoot", null, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.88f));
            var rootView = root.AddComponent<PostBattleRootView>();
            RewardChoicePanelView reward = BuildRewardPanel(root.transform);
            RouteChoicePanelView route = BuildRoutePanel(root.transform);
            RunEndPanelView end = BuildRunEndPanel(root.transform);
            SetReference(rootView, "rewardPanel", reward);
            SetReference(rootView, "routePanel", route);
            SetReference(rootView, "runEndPanel", end);
            reward.gameObject.SetActive(true);
            route.gameObject.SetActive(false);
            end.gameObject.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static RewardChoicePanelView BuildRewardPanel(Transform parent)
        {
            GameObject panel = CreateImage("RewardPanel", parent, Iron, true, Vector2.zero, Vector2.one);
            var view = panel.AddComponent<RewardChoicePanelView>();
            TMP_Text title = CreateText("Title", panel.transform, "戦闘報酬を選んでください", 36f,
                FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.03f, 0.84f), new Vector2(0.97f, 0.97f), Color.white);
            var slots = new List<RewardChoiceSlotView>();
            for (int i = 0; i < RewardChoicePanelView.RequiredSlotCount; i++)
            {
                float left = 0.035f + i * 0.325f;
                slots.Add(BuildRewardSlot(panel.transform, i, new Vector2(left, 0.09f), new Vector2(left + 0.29f, 0.80f)));
            }
            SetReference(view, "panelRoot", panel);
            SetReference(view, "titleText", title);
            SetArray(view, "slots", slots.ToArray());
            return view;
        }

        private static RewardChoiceSlotView BuildRewardSlot(Transform parent, int index, Vector2 min, Vector2 max)
        {
            Color color = index == 0 ? Red : index == 1 ? Green : Gold;
            GameObject slot = CreateImage($"RewardSlot_{index + 1:00}", parent, color, true, min, max);
            var view = slot.AddComponent<RewardChoiceSlotView>();
            Button button = slot.AddComponent<Button>();
            button.targetGraphic = slot.GetComponent<Image>();
            TMP_Text type = CreateText("Type", slot.transform, index == 0 ? "MOD付与" : index == 1 ? "HP回復" : "レベルアップ",
                25f, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.06f, 0.82f), new Vector2(0.94f, 0.96f), Ink);
            TMP_Text title = CreateText("Title", slot.transform, "報酬名", 29f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(0.06f, 0.61f), new Vector2(0.94f, 0.81f), Ink);
            TMP_Text description = CreateText("Description", slot.transform, "報酬の効果説明", 21f, FontStyles.Normal,
                TextAlignmentOptions.Top, new Vector2(0.08f, 0.17f), new Vector2(0.92f, 0.60f), Ink);
            TMP_Text unavailable = CreateText("Unavailable", slot.transform, "選択できません", 18f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.16f), Ink);
            unavailable.gameObject.SetActive(false);
            SetReference(view, "frameImage", slot.GetComponent<Image>());
            SetReference(view, "typeText", type);
            SetReference(view, "titleText", title);
            SetReference(view, "descriptionText", description);
            SetReference(view, "unavailableText", unavailable);
            SetReference(view, "selectButton", button);
            return view;
        }

        private static RouteChoicePanelView BuildRoutePanel(Transform parent)
        {
            GameObject panel = CreateImage("RouteChoicePanel", parent, Iron, true, Vector2.zero, Vector2.one);
            var view = panel.AddComponent<RouteChoicePanelView>();
            GameObject paper = CreateImage("Paper", panel.transform, Parchment, false,
                new Vector2(0.12f, 0.10f), new Vector2(0.88f, 0.90f));
            TMP_Text title = CreateText("Title", paper.transform, "遠征ボスを撃破した", 39f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(0.06f, 0.76f), new Vector2(0.94f, 0.94f), Ink);
            TMP_Text status = CreateText("Status", paper.transform, "現在HP 80    OverHunt深度 0\n保留ゴールド 120 G", 27f,
                FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.08f, 0.49f), new Vector2(0.92f, 0.74f), Ink);
            TMP_Text warning = CreateText("Warning", paper.transform, "続行中に死亡すると、このRUNは終了します。", 22f,
                FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.08f, 0.36f), new Vector2(0.92f, 0.48f), Red);
            Button returnButton = CreateButton("ReturnButton", paper.transform, "Outpostへ帰還", new Vector2(0.10f, 0.10f), new Vector2(0.45f, 0.29f));
            Button continueButton = CreateButton("ContinueButton", paper.transform, "OverHuntへ進む", new Vector2(0.55f, 0.10f), new Vector2(0.90f, 0.29f));
            TMP_Text continueLabel = continueButton.GetComponentInChildren<TMP_Text>();
            SetReference(view, "panelRoot", panel);
            SetReference(view, "titleText", title);
            SetReference(view, "statusText", status);
            SetReference(view, "warningText", warning);
            SetReference(view, "continueLabelText", continueLabel);
            SetReference(view, "returnButton", returnButton);
            SetReference(view, "continueButton", continueButton);
            return view;
        }

        private static RunEndPanelView BuildRunEndPanel(Transform parent)
        {
            GameObject panel = CreateImage("RunEndPanel", parent, Iron, true, Vector2.zero, Vector2.one);
            var view = panel.AddComponent<RunEndPanelView>();
            GameObject paper = CreateImage("Paper", panel.transform, Parchment, false,
                new Vector2(0.18f, 0.16f), new Vector2(0.82f, 0.84f));
            Image accent = CreateImage("Accent", paper.transform, Red, false,
                new Vector2(0f, 0.92f), Vector2.one).GetComponent<Image>();
            TMP_Text message = CreateText("Message", paper.transform, "致命的な傷を負った", 43f, FontStyles.Bold,
                TextAlignmentOptions.Center, new Vector2(0.08f, 0.48f), new Vector2(0.92f, 0.84f), Ink);
            Button action = CreateButton("ActionButton", paper.transform, "目の前が暗くなる",
                new Vector2(0.20f, 0.13f), new Vector2(0.80f, 0.35f));
            TMP_Text label = action.GetComponentInChildren<TMP_Text>();
            SetReference(view, "panelRoot", panel);
            SetReference(view, "messageText", message);
            SetReference(view, "actionLabelText", label);
            SetReference(view, "actionButton", action);
            SetReference(view, "accentImage", accent);
            return view;
        }

        /// <summary>旧動的パネルを除去し、完成済みPrefabとBattleUI参照をSceneへ保存する。</summary>
        private static void InstallIntoScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Canvas canvas = FindSceneComponent<Canvas>(scene);
            BattleUI battleUi = FindSceneComponent<BattleUI>(scene);
            if (canvas == null || battleUi == null)
                throw new InvalidOperationException("BattleSceneにCanvasまたはBattleUIがありません。");

            DestroyChild(canvas.transform, "RewardsPanel");
            DestroyChild(canvas.transform, "EndRunPanel");
            DestroyChild(canvas.transform, "PostBattleRoot");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath)
                ?? throw new InvalidOperationException("PostBattleRoot Prefabを読み込めません。");
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.name = "PostBattleRoot";
            instance.transform.SetParent(canvas.transform, false);
            instance.transform.SetAsLastSibling();
            SetReference(battleUi, "postBattleView", instance.GetComponent<PostBattleRootView>());
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static T FindSceneComponent<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
        }

        private static void DestroyChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 min, Vector2 max)
        {
            GameObject root = CreateImage(name, parent, new Color(0.28f, 0.21f, 0.14f, 1f), true, min, max);
            var button = root.AddComponent<Button>();
            button.targetGraphic = root.GetComponent<Image>();
            CreateText("Label", root.transform, label, 24f, FontStyles.Bold, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, Color.white);
            return button;
        }

        private static TMP_Text CreateText(string name, Transform parent, string text, float size, FontStyles style,
            TextAlignmentOptions alignment, Vector2 min, Vector2 max, Color color)
        {
            GameObject root = CreateRect(name, parent, min, max);
            var label = root.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = alignment;
            label.color = color;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            return label;
        }

        private static GameObject CreateImage(string name, Transform parent, Color color, bool raycast, Vector2 min, Vector2 max)
        {
            GameObject root = CreateRect(name, parent, min, max);
            var image = root.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return root;
        }

        private static GameObject CreateRect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var root = new GameObject(name, typeof(RectTransform));
            if (parent != null) root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            return root;
        }

        private static void SetReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException($"{target.GetType().Name}.{propertyName}がありません。");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray<T>(UnityEngine.Object target, string propertyName, T[] values) where T : UnityEngine.Object
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException($"{target.GetType().Name}.{propertyName}がありません。");
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureDirectory(string directory)
        {
            string current = "Assets";
            foreach (string segment in directory.Substring("Assets/".Length).Split('/'))
            {
                string next = current + "/" + segment;
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, segment);
                current = next;
            }
        }
    }
}
