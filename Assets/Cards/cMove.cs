using System.Collections.Generic;
using TestCardGame.Cards.Effects;
using TestCardGame.Cards.Modifiers;
using TestCardGame.Cards.VOs;

namespace TestCardGame.Cards
{
    public class cMove : CardBase
    {
        public cMove(CardLevel level, List<CardModifier> enchants = null): base
            (
                "移動",
                "ユニットを移動させる。",
                0,
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