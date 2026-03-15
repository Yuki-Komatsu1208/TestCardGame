using TestCardGame.Cards.Core;

namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// カードの効果を変更するための基底クラス
    /// </summary>
    public abstract class CardModifier
    {
        public virtual void OnMove(CardContext context) { }
        public virtual void OnRemove(CardContext context) { }
        public virtual void OnTurnStart(CardContext context) { }
        public virtual void OnTurnEnd(CardContext context) { }
        public virtual void OnBeforeEffect(CardContext context) { }
        public virtual void OnAfterEffect(CardContext context) { }

    }
}