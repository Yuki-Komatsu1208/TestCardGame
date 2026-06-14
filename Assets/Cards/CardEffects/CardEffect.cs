using TestCardGame.Cards.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestCardGame.Cards.Effects
{
    /// <summary>
    /// ゲーム内のカードの基本クラス
    /// カードの発動効果はこのクラスを継承する
    /// </summary>
    public abstract class CardEffect
    {
        /// <summary>
        /// 効果を発動。
        /// </summary>
        /// <param name="context"></param>
        public virtual void Execute(CardContext context){}
        public virtual void Execute(CardContext context, Vector2Int targetPosition){}
    }
}