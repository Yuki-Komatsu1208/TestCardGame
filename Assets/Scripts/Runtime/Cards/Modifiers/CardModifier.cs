using TestCardGame.Actions.Core;

namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// カードの効果を変更するための基底クラス
    /// </summary>
    public abstract class CardModifier
    {
        public virtual void OnMove(ActionContext context) { }
        public virtual void OnRemove(ActionContext context) { }
        public virtual void OnTurnStart(ActionContext context) { }
        public virtual void OnTurnEnd(ActionContext context) { }
        public virtual void OnBeforeEffect(ActionContext context) { }
        public virtual void OnAfterEffect(ActionContext context) { }

    }
}
