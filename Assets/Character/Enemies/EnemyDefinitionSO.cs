using System;
using System.Collections.Generic;
using UnityEngine;
using TestCardGame.Cards.Core;

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
        public EffectConfig effect;
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
