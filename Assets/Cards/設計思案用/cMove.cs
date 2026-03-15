

using TestCardGame.Cards.Core;
using TestCardGame.Cards.Effects;

namespace TestCardGame.Cards.設計試案用
{
    public class cMove : CardBase
    {
        public cMove(CardAssetBase data) : base(data)
        {
            Effects.Add(new eMove(Level));
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