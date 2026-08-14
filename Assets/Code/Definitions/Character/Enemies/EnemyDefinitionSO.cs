using System;
using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using UnityEngine;

namespace TestCardGame.Character.Enemies
{
    public enum BehaviorPattern
    {
        FirstExecutable,
        SequentialLoop
    }

    public enum EnemyTargetKind
    {
        Self,
        Target
    }

    /// <summary>
    /// 敵行動をプレイヤーへ予告する際の表示分類。
    /// Unsetはデータ設定漏れの検出専用で、実行時表示には使用しない。
    /// </summary>
    public enum EnemyIntentKind
    {
        Unset,
        Attack,
        StatusEffect,
        Other
    }

    [Serializable]
    public struct EnemyActionPlanConfig
    {
        public EnemyTargetKind targetKind;
        public EnemyIntentKind intentKind;
        public ActionEffectSO effect;
        public bool useCustomParameters;
        public ActionEffectParameters parameters;
    }

    [CreateAssetMenu(fileName = "NewEnemyDefinition", menuName = "Card Game/Enemy Definition")]
    public class EnemyDefinitionSO : ScriptableObject
    {
        public string enemyName;
        public int maxHp;
        [Min(1)] public int maxStunResist = 100;
        public int characterCode;
        public Color enemyColor = Color.red;
        public BehaviorPattern behaviorPattern;
        public List<EnemyActionPlanConfig> actionPlans;
    }
}
