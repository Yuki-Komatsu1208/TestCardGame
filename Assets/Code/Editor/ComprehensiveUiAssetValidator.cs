using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TestCardGame.BoardManage;
using TestCardGame.Controller;
using TestCardGame.UIs;
using TestCardGame.UIs.Battle;
using TestCardGame.UIs.PostBattle;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TestCardGame.Editor
{
    /// <summary>ADR-017のScene、Prefab、固定枠、実行時UI生成禁止を一括検査する。</summary>
    public static class ComprehensiveUiAssetValidator
    {
        private const string OutpostScenePath = "Assets/GameData/Scenes/OutpostScene.unity";
        private const string BattleScenePath = "Assets/GameData/Scenes/BattleScene.unity";
        private static readonly string[] PrefabRoots = { "Assets/GameData/Prefabs" };

        [MenuItem("Tools/Test Card Game/UI/Validate All Serialized UI")]
        public static void ValidateAll()
        {
            var errors = new List<string>();
            ValidateBuildSettings(errors);
            ValidateRuntimeSource(errors);
            ValidatePrefabAssets(errors);
            ValidateOutpostScene(errors);
            ValidateBattleScene(errors);
            if (errors.Count > 0)
                throw new InvalidOperationException("PLAN08 総合UI検証:\n- " + string.Join("\n- ", errors));
            Debug.Log("PLAN08 総合UI検証: Scene、Prefab、固定枠、禁止API、Build Settingsに問題はありません。");
        }

        /// <summary>Build Settingsの開始SceneがOutpostで、Battleが後続に含まれることを検査する。</summary>
        private static void ValidateBuildSettings(List<string> errors)
        {
            EditorBuildSettingsScene[] enabled = EditorBuildSettings.scenes.Where(scene => scene.enabled).ToArray();
            if (enabled.Length == 0 || enabled[0].path != OutpostScenePath)
                errors.Add("Build Settingsの開始SceneはOutpostSceneである必要があります。");
            if (!enabled.Any(scene => scene.path == BattleScenePath))
                errors.Add("Build SettingsにBattleSceneがありません。");
        }

        /// <summary>UI関連Runtimeコードに禁止された生成・探索経路が残っていないことを検査する。</summary>
        private static void ValidateRuntimeSource(List<string> errors)
        {
            var files = new List<string>();
            string[] roots =
            {
                "Assets/Code/Runtime/UIs", "Assets/Code/Runtime/Controller",
                "Assets/Code/Runtime/Cards/Views"
            };
            foreach (string root in roots)
                if (Directory.Exists(root)) files.AddRange(Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories));
            files.Add("Assets/Code/Runtime/GameStarter.cs");

            var forbidden = new Dictionary<string, Regex>
            {
                { "new GameObject", new Regex(@"\bnew\s+GameObject\s*\(") },
                { "AddComponent", new Regex(@"\bAddComponent\s*<") },
                { "Instantiate", new Regex(@"\bInstantiate\s*\(") },
                { "Destroy", new Regex(@"\bDestroy\s*\(") },
                { "Scene全体探索", new Regex(@"\b(?:FindAnyObjectByType|FindObjectOfType)\s*<") },
                { "reflection", new Regex(@"\b(?:GetField|GetProperty)\s*\(") }
            };
            foreach (string file in files.Where(File.Exists))
            {
                string source = File.ReadAllText(file);
                foreach (var entry in forbidden)
                    if (entry.Value.IsMatch(source))
                        errors.Add($"{file}: 実行時UIコードに{entry.Key}が残っています。");
            }

            string cellBuilder = File.ReadAllText("Assets/Code/Runtime/BoardManage/CellBuilder.cs");
            if (Regex.Matches(cellBuilder, @"\bInstantiate\s*\(").Count != 4)
                errors.Add("CellBuilderの許可済み生成経路数が変化しています（Cell Prefab 1経路、UnitView 3経路の想定）。");
            if (cellBuilder.Contains("new GameObject") || cellBuilder.Contains("AddComponent<"))
                errors.Add("CellBuilderが完成済みPrefab以外のUI構造を生成しています。");
        }

        /// <summary>全PrefabのMissing ScriptとMissing Referenceを検査する。</summary>
        private static void ValidatePrefabAssets(List<string> errors)
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", PrefabRoots))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                try { ValidateHierarchy(root, path, errors); }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
        }

        private static void ValidateOutpostScene(List<string> errors)
        {
            Scene scene = EditorSceneManager.OpenScene(OutpostScenePath, OpenSceneMode.Single);
            ValidateSceneCommon(scene, OutpostScenePath, errors);
            OutpostSceneController controller = Single<OutpostSceneController>(scene, OutpostScenePath, errors);
            OutpostRootView view = Single<OutpostRootView>(scene, OutpostScenePath, errors);
            if (controller != null && !controller.ValidateReferences()) errors.Add("OutpostSceneControllerの必須参照が不足しています。");
            if (view != null)
            {
                if (!view.ValidateReferences()) errors.Add("OutpostRootViewの必須参照が不足しています。");
                ValidateUniqueArray(view.Shop, "cardSlots", 10, "ショップカード", errors);
                ValidateUniqueArray(view.Shop, "keystoneSlots", 3, "キーストーン", errors);
                foreach (string panel in new[] { "FacilitiesPanel", "InnPanel", "ShopPanel", "PreparingPanel" })
                    if (view.transform.Find(panel) == null) errors.Add($"Outpostに{panel}がありません。");
            }
            ValidateLegacyNames(scene, new[] { "KeystoneSelectionPanel", "OldTownPanel", "TownPanel" }, errors);
        }

        private static void ValidateBattleScene(List<string> errors)
        {
            Scene scene = EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
            ValidateSceneCommon(scene, BattleScenePath, errors);
            GameController gameController = Single<GameController>(scene, BattleScenePath, errors);
            RunController runController = Single<RunController>(scene, BattleScenePath, errors);
            RewardController rewardController = Single<RewardController>(scene, BattleScenePath, errors);
            GameStarter starter = Single<GameStarter>(scene, BattleScenePath, errors);
            CellBuilder builder = Single<CellBuilder>(scene, BattleScenePath, errors);
            HandView hand = Single<HandView>(scene, BattleScenePath, errors);
            BattleHudView hud = Single<BattleHudView>(scene, BattleScenePath, errors);
            BattleUI battleUi = Single<BattleUI>(scene, BattleScenePath, errors);
            BoardView board = Single<BoardView>(scene, BattleScenePath, errors);
            CardListSelectionOverlay overlay = Single<CardListSelectionOverlay>(scene, BattleScenePath, errors);
            PostBattleRootView postBattle = Single<PostBattleRootView>(scene, BattleScenePath, errors);
            EnemyIntentPresenter intent = Single<EnemyIntentPresenter>(scene, BattleScenePath, errors);

            ValidateRequiredReferences(gameController, new[] { "cellBuilder", "handView", "boardView", "battleUIInstance" }, errors);
            ValidateRequiredReferences(runController, new[] { "runDefinition", "gameController", "handView", "rewardController" }, errors);
            ValidateRequiredReferences(rewardController, new[] { "gameController" }, errors);
            ValidateRequiredReferences(starter, new[] { "gameController", "handView", "runController" }, errors);
            ValidateRequiredReferences(builder, new[] { "cellPrefab", "playerView", "playerDefinition", "enemyDefinition", "boardView", "tooltipView" }, errors);
            if (battleUi != null && !battleUi.ValidateReferences()) errors.Add("BattleUIの必須参照が不足しています。");
            if (board != null && !board.ValidateReferences()) errors.Add("BoardViewの必須参照が不足しています。");
            if (intent != null && !intent.ValidateReferences()) errors.Add("EnemyIntentPresenterの必須参照が不足しています。");
            if (hand != null)
            {
                if (!hand.ValidateReferences()) errors.Add("HandViewの必須参照が不足しています。");
                ValidateUniqueArray(hand, "slots", 50, "戦闘手札", errors);
                ValidateRequiredReferences(hand, new[] { "selectionHeaderRoot", "selectionTitleText",
                    "selectionCountText", "selectionOkButton", "selectionCancelButton" }, errors);
            }
            if (hud != null)
            {
                if (!hud.ValidateReferences()) errors.Add("BattleHudViewの必須参照が不足しています。");
                ValidateUniqueArray(hud, "enemySlots", 12, "敵HUD", errors);
            }
            if (overlay != null)
            {
                ValidateUniqueArray(overlay, "slots", 50, "カード一覧", errors);
                ValidateRequiredReferences(overlay, new[] { "scrollRect", "detailPanel", "selectionCountText", "okButton", "cancelButton", "enhancementModal" }, errors);
                ScrollRect scroll = new SerializedObject(overlay).FindProperty("scrollRect")?.objectReferenceValue as ScrollRect;
                GridLayoutGroup grid = overlay.GetComponentInChildren<GridLayoutGroup>(true);
                if (scroll == null || !scroll.vertical) errors.Add("カード一覧は縦ScrollRectである必要があります。");
                if (grid == null || grid.constraint != GridLayoutGroup.Constraint.FixedColumnCount || grid.constraintCount != 6)
                    errors.Add("カード一覧は6列固定である必要があります。");
            }
            if (postBattle != null)
            {
                if (!postBattle.ValidateReferences()) errors.Add("PostBattleRootViewの必須参照が不足しています。");
                if (postBattle.RewardPanel != null) ValidateUniqueArray(postBattle.RewardPanel, "slots", 3, "通常報酬", errors);
            }
            ValidateLegacyNames(scene, new[] { "RewardsPanel", "EndRunPanel", "KeystoneSelectionPanel", "TownPanel" }, errors);
        }

        /// <summary>Canvas、Scaler、EventSystem、Camera、Missing参照をScene共通要件として検査する。</summary>
        private static void ValidateSceneCommon(Scene scene, string path, List<string> errors)
        {
            foreach (GameObject root in scene.GetRootGameObjects()) ValidateHierarchy(root, path, errors);
            Canvas[] canvases = FindSceneComponents<Canvas>(scene).Where(value => value.GetComponent<CanvasScaler>() != null).ToArray();
            if (canvases.Length != 1) errors.Add($"{path}: 基準Canvasは1件必要です。現在{canvases.Length}件です。");
            else
            {
                CanvasScaler scaler = canvases[0].GetComponent<CanvasScaler>();
                if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize || scaler.referenceResolution != new Vector2(1920f, 1080f))
                    errors.Add($"{path}: CanvasScalerは1920 x 1080基準である必要があります。");
                float minimumScale = Mathf.Min(1280f / scaler.referenceResolution.x, 720f / scaler.referenceResolution.y);
                if (minimumScale < 0.66f) errors.Add($"{path}: 1280 x 720で想定縮尺を維持できません。");
            }
            if (FindSceneComponents<EventSystem>(scene).Length != 1) errors.Add($"{path}: EventSystemは1件必要です。");
            if (FindSceneComponents<Camera>(scene).Length < 1) errors.Add($"{path}: Cameraがありません。");
            if (FindSceneComponents<CommonUiController>(scene).Length != 1) errors.Add($"{path}: CommonUiControllerは1件必要です。");
        }

        private static void ValidateHierarchy(GameObject root, string context, List<string> errors)
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) > 0)
                    errors.Add($"{context}: {GetPath(transform)} にMissing Scriptがあります。");
                foreach (Component component in transform.GetComponents<Component>())
                {
                    if (component == null) continue;
                    var serialized = new SerializedObject(component);
                    SerializedProperty iterator = serialized.GetIterator();
                    if (!iterator.NextVisible(true)) continue;
                    do
                    {
                        if (iterator.propertyType == SerializedPropertyType.ObjectReference &&
                            iterator.objectReferenceValue == null &&
                            !iterator.objectReferenceEntityIdValue.Equals(default(UnityEngine.EntityId)))
                            errors.Add($"{context}: {GetPath(transform)}.{component.GetType().Name}.{iterator.propertyPath} がMissing Referenceです。");
                    } while (iterator.NextVisible(false));
                }
            }
        }

        private static void ValidateUniqueArray(UnityEngine.Object target, string propertyName, int expected, string label, List<string> errors)
        {
            if (target == null) return;
            SerializedProperty property = new SerializedObject(target).FindProperty(propertyName);
            if (property == null || !property.isArray || property.arraySize != expected)
            {
                errors.Add($"{label}は{expected}枠必要です。");
                return;
            }
            var unique = new HashSet<UnityEngine.Object>();
            for (int index = 0; index < property.arraySize; index++)
            {
                UnityEngine.Object value = property.GetArrayElementAtIndex(index).objectReferenceValue;
                if (value == null) errors.Add($"{label}{index + 1}枠目の参照がありません。");
                else if (!unique.Add(value)) errors.Add($"{label}{index + 1}枠目の参照が重複しています。");
            }
        }

        private static void ValidateRequiredReferences(UnityEngine.Object target, IEnumerable<string> names, List<string> errors)
        {
            if (target == null) return;
            var serialized = new SerializedObject(target);
            foreach (string name in names)
            {
                SerializedProperty property = serialized.FindProperty(name);
                if (property == null || property.objectReferenceValue == null)
                    errors.Add($"{target.GetType().Name}.{name}のInspector参照がありません。");
            }
        }

        private static T Single<T>(Scene scene, string context, List<string> errors) where T : Component
        {
            T[] values = FindSceneComponents<T>(scene);
            if (values.Length != 1)
            {
                errors.Add($"{context}: {typeof(T).Name}は1件必要です。現在{values.Length}件です。");
                return null;
            }
            return values[0];
        }

        private static T[] FindSceneComponents<T>(Scene scene) where T : Component
        {
            var results = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects()) results.AddRange(root.GetComponentsInChildren<T>(true));
            return results.ToArray();
        }

        private static void ValidateLegacyNames(Scene scene, IEnumerable<string> names, List<string> errors)
        {
            var legacy = new HashSet<string>(names);
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    if (legacy.Contains(child.name)) errors.Add($"{scene.path}: 旧UI経路 {child.name} が残っています。");
        }

        private static string GetPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null) { transform = transform.parent; path = transform.name + "/" + path; }
            return path;
        }
    }

    /// <summary>Player Build前に総合UI検証を必須実行する。</summary>
    public sealed class ComprehensiveUiBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        public void OnPreprocessBuild(BuildReport report) => ComprehensiveUiAssetValidator.ValidateAll();
    }
}
