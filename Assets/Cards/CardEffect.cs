using UnityEngine;
using UnityEngine.EventSystems;

namespace TestCardGame.Cards
{
    /// <summary>
    /// ゲーム内のカードの基本クラス
    /// </summary>
    public abstract class CardEffect
    {
        public virtual void Execute(CardContext context){}
        public virtual void Execute(CardContext context, Vector2Int targetPosition){}
    }
}