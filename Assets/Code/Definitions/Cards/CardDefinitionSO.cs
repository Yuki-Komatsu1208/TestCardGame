using System;
using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using TestCardGame.Cards.VOs;
using TestCardGame.Run;
using TestCardGame.Cards.Core.Modifiers;
using UnityEngine;

namespace TestCardGame.Cards.Core
{
    [Serializable]
    public class CardLevelData
    {
        public string description;
        public ManaCost cost = ManaCost.Zero;
        public CardCooldown cooldown = CardCooldown.None;
        public List<CardEffectEntry> effects;

        public ManaCost Cost => cost ?? ManaCost.Zero;
        public CardCooldown Cooldown => cooldown ?? CardCooldown.None;
    }

    [CreateAssetMenu(fileName = "NewCardDefinition", menuName = "Card Game/Card Definition")]
    public class CardDefinitionSO : ScriptableObject
    {
        public string cardName;
        public List<BuildTag> buildTags = new();
        [Tooltip("このカード自身が常に持つModifier。報酬で付与するEnchantとは別に、カード固有の性質を表す。")]
        public List<CardModifierSO> intrinsicModifiers = new();
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
