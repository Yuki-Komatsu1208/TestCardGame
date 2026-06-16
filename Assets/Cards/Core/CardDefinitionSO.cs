using System;
using System.Collections.Generic;
using UnityEngine;
using TestCardGame.Actions.Effects;
using TestCardGame.Actions.Core;

namespace TestCardGame.Cards.Core
{
    public enum EffectType
    {
        Move,
        LineAttack,
        Ignite,
        PositionAttack,
        Sleep,
        IgniteAround
    }

    [Serializable]
    public struct EffectConfig
    {
        public EffectType effectType;
        public int param1;
        public int param2;
    }

    [Serializable]
    public class CardLevelData
    {
        public string description;
        public int cost;
        public List<EffectConfig> effects;
    }

    [CreateAssetMenu(fileName = "NewCardDefinition", menuName = "Card Game/Card Definition")]
    public class CardDefinitionSO : ScriptableObject
    {
        public string cardName;
        public CardLevelData level1;
        public CardLevelData level2;
        public CardLevelData level3;

        public CardLevelData GetDataForLevel(int level)
        {
            switch (level)
            {
                case 1: return level1;
                case 2: return level2;
                case 3: return level3;
                default: throw new ArgumentOutOfRangeException(nameof(level), "レベルは1～3の範囲で指定してください。");
            }
        }
    }

    public static class EffectFactory
    {
        public static ActionEffect CreateEffect(EffectConfig config)
        {
            switch (config.effectType)
            {
                case EffectType.Move:
                    return new eMove(config.param1);
                case EffectType.LineAttack:
                    return new eLineAttack(config.param1, config.param2, HitType.FirstTargetOnly);
                case EffectType.Ignite:
                    return new eIgnite(config.param1, config.param2);
                case EffectType.PositionAttack:
                    return new ePositionAttack(config.param1, config.param2 > 0 ? (int?)config.param2 : null);
                case EffectType.Sleep:
                    return new eSleep();
                case EffectType.IgniteAround:
                    return new eIgniteAround(config.param1, config.param2);
                default:
                    throw new ArgumentOutOfRangeException(nameof(config.effectType), $"未定義の効果タイプです: {config.effectType}");
            }
        }
    }
}
