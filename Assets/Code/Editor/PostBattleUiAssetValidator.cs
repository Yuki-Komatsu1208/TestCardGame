using System;
using System.Collections.Generic;
using TestCardGame.Controller;
using TestCardGame.UIs.PostBattle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestCardGame.Editor
{
    /// <summary>PLAN07の固定3枠とScene参照、旧動的パネル除去を検証する。</summary>
    public static class PostBattleUiAssetValidator
    {
        private const string ScenePath = "Assets/GameData/Scenes/BattleScene.unity";

        [MenuItem("Tools/Test Card Game/UI/Validate Post Battle And Recovery")]
        public static void Validate()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var errors = new List<string>();
            PostBattleRootView[] roots = FindSceneComponents<PostBattleRootView>(scene);
            BattleUI[] battleUis = FindSceneComponents<BattleUI>(scene);
            if (roots.Length != 1) errors.Add($"PostBattleRootViewは1件必要です。現在: {roots.Length}");
            if (battleUis.Length != 1) errors.Add($"BattleUIは1件必要です。現在: {battleUis.Length}");
            if (roots.Length == 1 && !roots[0].ValidateReferences()) errors.Add("PostBattleRootViewの参照が不足しています。");
            if (roots.Length == 1 && roots[0].RewardPanel == null) errors.Add("報酬パネルがありません。");
            if (FindSceneObject(scene, "RewardsPanel") != null) errors.Add("旧RewardsPanelが残っています。");
            if (FindSceneObject(scene, "EndRunPanel") != null) errors.Add("旧EndRunPanelが残っています。");
            if (battleUis.Length == 1)
            {
                var serialized = new SerializedObject(battleUis[0]);
                if (serialized.FindProperty("postBattleView")?.objectReferenceValue == null)
                    errors.Add("BattleUI.postBattleViewが未設定です。");
            }

            if (errors.Count > 0)
                throw new InvalidOperationException("PLAN07 Validator:\n- " + string.Join("\n- ", errors));
            Debug.Log("PLAN07 Validator: 報酬3枠、経路選択、RUN終了、BattleUI参照を確認しました。");
        }

        private static T[] FindSceneComponents<T>(Scene scene) where T : Component
        {
            var results = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
                results.AddRange(root.GetComponentsInChildren<T>(true));
            return results.ToArray();
        }

        private static GameObject FindSceneObject(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    if (child.name == name) return child.gameObject;
            return null;
        }
    }
}
