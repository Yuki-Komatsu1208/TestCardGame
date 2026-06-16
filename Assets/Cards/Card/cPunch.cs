using System.Collections.Generic;
using TestCardGame.Actions.Effects;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.Modifiers;
using TestCardGame.Cards.VOs;

namespace TestCardGame.Cards.Card
{
    /// <summary>
    /// 前方１マスに対してダメージを与える。
    /// </summary>
    public class cPunch : CardBase
    {
        private static readonly string _cardName = "パンチ";
        private static readonly string _description = "前方一マスの敵にパンチする（ダメージ: 10）";
        private static readonly int _baseCost = 1;
        private static readonly int _baseDamage = 10;
        public cPunch(CardLevel level,  List<CardModifier> enchants = null) 
            : base
            (
                _cardName,
                _description, 
                _baseCost, 
                level, 
                new List<ActionEffect>
                {
                    new eLineAttack(_baseDamage + level * 5, 1, HitType.FirstTargetOnly)
                },
                enchants)
        {
        }
        /// <summary>
        /// 攻撃力＝（１０＋レベル×５）
        /// </summary>
        public override void LevelUp()
        {
            base.LevelUp();
            Effects[0] = new eLineAttack(_baseDamage + Level * 5, 1, HitType.FirstTargetOnly);
        }
        /// <summary>
        /// 攻撃力＝（１０＋レベル×５）
        /// </summary>
        public override void LevelDown()
        {
            base.LevelDown();
            Effects[0] = new eLineAttack(_baseDamage + Level * 5, 1, HitType.FirstTargetOnly);
        }
    }
}
