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

    [Serializable]
    public struct EnemyActionPlanConfig
    {
        public EnemyTargetKind targetKind;
        public ActionEffectSO effect;
        public bool useCustomParameters;
        public ActionEffectParameters parameters;
    }

    [CreateAssetMenu(fileName = "NewEnemyDefinition", menuName = "Card Game/Enemy Definition")]
    public class EnemyDefinitionSO : ScriptableObject
    {
        public string enemyName;
        public int maxHp;
        public int characterCode;
        public Color enemyColor = Color.red;
        public BehaviorPattern behaviorPattern;
        public List<EnemyActionPlanConfig> actionPlans;
    }
}
