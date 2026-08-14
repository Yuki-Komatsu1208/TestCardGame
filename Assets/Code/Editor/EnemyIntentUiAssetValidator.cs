using System;
using TestCardGame.Character;
using TestCardGame.Character.Enemies;
using TestCardGame.UIs.Battle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestCardGame.Editor
{
    /// <summary>PLAN06の全敵分類とScene内の完成済みView参照を検証する。</summary>
    public static class EnemyIntentUiAssetValidator
    {
        [MenuItem("Tools/Test Card Game/UI/Validate Enemy Intent Preview")]
        public static void Validate()
        {
            string[] guids = AssetDatabase.FindAssets("t:EnemyDefinitionSO", new[] { "Assets/GameData/Character/Enemies" });
            if (guids.Length != 10) throw new InvalidOperationException($"敵定義は10件の想定ですが、{guids.Length}件です。");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                EnemyDefinitionSO definition = AssetDatabase.LoadAssetAtPath<EnemyDefinitionSO>(path);
                if (definition?.actionPlans == null) throw new InvalidOperationException($"{path}に行動計画がありません。");
                for (int index = 0; index < definition.actionPlans.Count; index++)
                {
                    EnemyActionPlanConfig plan = definition.actionPlans[index];
                    if (plan.effect == null || plan.intentKind == EnemyIntentKind.Unset)
                        throw new InvalidOperationException($"{path} のPlan {index + 1}にEffectまたは意図分類がありません。");
                }
            }

            Scene scene = EditorSceneManager.OpenScene("Assets/GameData/Scenes/BattleScene.unity", OpenSceneMode.Single);
            EnemyIntentView view = Find<EnemyIntentView>(scene);
            EnemyIntentPresenter presenter = Find<EnemyIntentPresenter>(scene);
            if (view == null || !view.ValidateReferences())
                throw new InvalidOperationException("敵View複製元のEnemyIntentView参照が不足しています。");
            if (presenter == null || !presenter.ValidateReferences())
                throw new InvalidOperationException("EnemyIntentPresenterのScene参照が不足しています。");
            Debug.Log("PLAN06 Validator: 全10敵の行動分類と敵意図View/Presenter参照を確認しました。");
        }

        private static T Find<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null) return component;
            }
            return null;
        }
    }
}
