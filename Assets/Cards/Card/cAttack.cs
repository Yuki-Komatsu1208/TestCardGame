using System.Collections.Generic;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.Effects;
using TestCardGame.Cards.Modifiers;
using TestCardGame.Cards.VOs;

namespace TestCardGame.Cards.Card
{
    public class cAttack : CardBase
    {
        private static readonly string _cardName = "攻撃";
        private static readonly int _baseCost = 1;

        public cAttack(CardLevel level, List<CardModifier> enchants = null) : base(
            _cardName,
            $"前方一マスの敵に攻撃する（ダメージ: {GetDamage(level)}）",
            _baseCost,
            level,
            new List<CardEffect> { new eAttack(GetDamage(level)) },
            enchants
        )
        {
        }

        private static int GetDamage(CardLevel level)
        {
            return level.Level * 15; // Level 1: 15, Level 2: 30, Level 3: 45
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
            Description = $"前方一マスの敵に攻撃する（ダメージ: {GetDamage(Level)}）";
            Effects[0] = new eAttack(GetDamage(Level));
        }
    }
}