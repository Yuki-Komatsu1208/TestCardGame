using System;
using System.Collections.Generic;
using TestCardGame.BoardManage;
using TestCardGame.Character;
using TestCardGame.Character.Enemies;
using TestCardGame.Controller;
using TestCardGame.UIs.Battle;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TestCardGame.Editor
{
    /// <summary>PLAN06の敵意図データと完成済み頭上UIを構築する。</summary>
    public static class EnemyIntentUiAssetBuilder
    {
        private const string ScenePath = "Assets/GameData/Scenes/BattleScene.unity";
        private const string EnemyFolder = "Assets/GameData/Character/Enemies/";

        private static readonly IReadOnlyDictionary<string, EnemyIntentKind[]> IntentTable =
            new Dictionary<string, EnemyIntentKind[]>
            {
                ["AccumulatorEnemy.asset"] = new[] { EnemyIntentKind.StatusEffect, EnemyIntentKind.Attack, EnemyIntentKind.Attack },
                ["DragonEnemy.asset"] = new[] { EnemyIntentKind.Other, EnemyIntentKind.Attack, EnemyIntentKind.Attack },
                ["FireSlimeEnemy.asset"] = new[] { EnemyIntentKind.Other, EnemyIntentKind.StatusEffect, EnemyIntentKind.StatusEffect },
                ["GoblinArcherEnemy.asset"] = new[] { EnemyIntentKind.Attack },
                ["GoblinFighterEnemy.asset"] = new[] { EnemyIntentKind.Other, EnemyIntentKind.Attack, EnemyIntentKind.Attack },
                ["GoblinKingEnemy.asset"] = new[] { EnemyIntentKind.Other, EnemyIntentKind.Attack },
                ["GolemEnemy.asset"] = new[] { EnemyIntentKind.Other, EnemyIntentKind.Attack, EnemyIntentKind.Other },
                ["MiniGoblinEnemy.asset"] = new[] { EnemyIntentKind.Attack },
                ["SlimeEnemy.asset"] = new[] { EnemyIntentKind.Attack, EnemyIntentKind.Other },
                ["SlimeKingEnemy.asset"] = new[] { EnemyIntentKind.StatusEffect, EnemyIntentKind.Attack }
            };

        [MenuItem("Tools/Test Card Game/UI/Build Enemy Intent Preview")]
        public static void BuildAll()
        {
            ApplyExplicitIntentData();
            InstallIntentViews();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("PLAN06: 敵次行動の明示データと頭上UIを構築しました。");
        }

        /// <summary>バッチ実行で構築後に必須参照と全敵データを検証する。</summary>
        public static void BuildAndValidateBatch()
        {
            BuildAll();
            EnemyIntentUiAssetValidator.Validate();
        }

        /// <summary>開発者確認済み分類表を各EnemyDefinitionの全Planへ保存する。</summary>
        private static void ApplyExplicitIntentData()
        {
            foreach (var pair in IntentTable)
            {
                string path = EnemyFolder + pair.Key;
                EnemyDefinitionSO definition = AssetDatabase.LoadAssetAtPath<EnemyDefinitionSO>(path)
                    ?? throw new InvalidOperationException($"敵定義が見つかりません: {path}");
                if (definition.actionPlans == null || definition.actionPlans.Count != pair.Value.Length)
                    throw new InvalidOperationException($"{pair.Key}のPlan数が確認済み分類表と一致しません。");

                for (int index = 0; index < pair.Value.Length; index++)
                {
                    EnemyActionPlanConfig config = definition.actionPlans[index];
                    config.intentKind = pair.Value[index];
                    definition.actionPlans[index] = config;
                }
                EditorUtility.SetDirty(definition);
            }
        }

        /// <summary>敵ビューの複製元へ意図領域を保存し、SceneへPresenterを接続する。</summary>
        private static void InstallIntentViews()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject player = FindSceneObject(scene, "Player")
                ?? throw new InvalidOperationException("BattleSceneにPlayer Viewがありません。");
            BoardView boardView = FindSceneComponent<BoardView>(scene)
                ?? throw new InvalidOperationException("BattleSceneにBoardViewがありません。");
            CellBuilder cellBuilder = FindSceneComponent<CellBuilder>(scene)
                ?? throw new InvalidOperationException("BattleSceneにCellBuilderがありません。");
            GameController controller = FindSceneComponent<GameController>(scene)
                ?? throw new InvalidOperationException("BattleSceneにGameControllerがありません。");

            DestroyChild(player.transform, "EnemyIntentText");
            DestroyChild(player.transform, "EnemyIntentSelectionBorder");
            EnemyIntentView view = player.GetComponent<EnemyIntentView>() ?? player.AddComponent<EnemyIntentView>();

            Image selection = CreateSelectionBorder(player.transform);
            TextMeshProUGUI intentText = CreateIntentText(player.transform);
            SetReference(view, "intentText", intentText);
            SetReference(view, "selectionBorder", selection);
            view.DisableIntent();

            Transform oldPresenter = boardView.transform.Find("EnemyIntentPresenter");
            if (oldPresenter != null) UnityEngine.Object.DestroyImmediate(oldPresenter.gameObject);
            var presenterObject = new GameObject("EnemyIntentPresenter", typeof(RectTransform), typeof(EnemyIntentPresenter));
            presenterObject.layer = 5;
            presenterObject.transform.SetParent(boardView.transform, false);
            EnemyIntentPresenter presenter = presenterObject.GetComponent<EnemyIntentPresenter>();
            SetReference(presenter, "controller", controller);
            SetReference(presenter, "cellBuilder", cellBuilder);
            SetReference(presenter, "boardView", boardView);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Image CreateSelectionBorder(Transform parent)
        {
            var gameObject = new GameObject("EnemyIntentSelectionBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            gameObject.layer = 5;
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(-4f, -4f);
            rect.offsetMax = new Vector2(4f, 4f);
            Image image = gameObject.GetComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            image.type = Image.Type.Sliced;
            image.fillCenter = false;
            image.color = new Color(0.95f, 0.70f, 0.16f, 1f);
            image.raycastTarget = false;
            gameObject.SetActive(false);
            return image;
        }

        private static TextMeshProUGUI CreateIntentText(Transform parent)
        {
            var gameObject = new GameObject("EnemyIntentText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            gameObject.layer = 5;
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 4f);
            rect.sizeDelta = new Vector2(42f, 34f);
            TextMeshProUGUI text = gameObject.GetComponent<TextMeshProUGUI>();
            text.text = "?";
            text.fontSize = 28f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(1f, 0.82f, 0.32f, 1f);
            text.raycastTarget = false;
            gameObject.SetActive(false);
            return text;
        }

        private static void DestroyChild(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
        }

        private static GameObject FindSceneObject(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    if (child.name == name) return child.gameObject;
            return null;
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

        private static void SetReference(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName)
                ?? throw new InvalidOperationException($"{target.GetType().Name}.{propertyName}がありません。");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
