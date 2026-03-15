using System.Collections.Generic;
using TestCardGame.Cards.Core;
using TestCardGame.Cards.Effects;
using TestCardGame.Cards.Modifiers;
using TestCardGame.Cards.VOs;
using UnityEditor;

namespace TestCardGame.Cards.Card
{
    public class cMove : CardBase
    {
        private static readonly string _cardName = "移動";
        private static readonly string _description = "ユニットを移動させる。";
        private static readonly int _baseCost = 1;
        public cMove(CardLevel level, List<CardModifier> enchants = null): base
            (
                _cardName,
                _description,
                _baseCost,
                level,
                new List<CardEffect> { new eMove(level) },
                enchants
            )
        {
        }
        /// <summary>
        /// 移動距離がレベルに応じて変化する
        /// </summary>
        public override void LevelUp()
        {
            base.LevelUp();
            Effects[0] = new eMove(Level);
            // レベルアップ時の効果をここに実装（必要に応じてオーバーライド）
        }

        /// <summary>
        /// 移動距離がレベルに応じて変化する
        /// </summary>
        public override void LevelDown()
        {
            base.LevelDown();
            Effects[0] = new eMove(Level);
            // レベルダウン時の効果をここに実装（必要に応じてオーバーライド）
        }
    }
}