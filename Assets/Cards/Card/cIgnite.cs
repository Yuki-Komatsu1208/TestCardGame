using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.Modifiers;
using TestCardGame.Cards.VOs;

namespace TestCardGame.Cards.Card
{
    public class cIgnite : CardBase
    {
        private static readonly string _cardName = "炎上";
        private static readonly int _baseCost = 1;

        public cIgnite(CardLevel level, List<CardModifier> enchants = null) : base(
            _cardName,
            $"任意の2マス先までに炎上効果を付与する（毎ターン{GetDamage(level)}ダメージ、{GetTurns(level)}ターン持続）",
            _baseCost,
            level,
            new List<ActionEffect> { new eIgnite(GetTurns(level), GetDamage(level)) },
            enchants
        )
        {
        }

        private static int GetDamage(CardLevel level)
        {
            return level.Level * 5; // Level 1: 5, Level 2: 10, Level 3: 15
        }

        private static int GetTurns(CardLevel level)
        {
            return level.Level + 1; // Level 1: 2 turns, Level 2: 3 turns, Level 3: 4 turns
        }

        public override void LevelUp()
        {
            base.LevelUp();
            UpdateEffect();
        }

        public override void LevelDown()
        {
            base.LevelDown();
            UpdateEffect();
        }

        private void UpdateEffect()
        {
            Description = $"任意の2マス先までに炎上効果を付与する（毎ターン{GetDamage(Level)}ダメージ、{GetTurns(Level)}ターン持続）";
            Effects[0] = new eIgnite(GetTurns(Level), GetDamage(Level));
        }
    }
}
