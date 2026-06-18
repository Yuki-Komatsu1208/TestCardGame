using System;
using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using UnityEngine;

namespace TestCardGame.Cards.Core
{
    [Serializable]
    public class CardLevelData
    {
        public string description;
        public int cost;
        public List<CardEffectEntry> effects;
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
}
