using TestCardGame.Cards.VOs;

namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// カードの効果を変更するための基底クラス
    /// </summary>
    public abstract class CardModifier
    {
        public virtual ManaCost ModifyCost(ManaCost currentCost, CardModifierContext context) => currentCost;
        public virtual CardCooldown ModifyCooldown(CardCooldown currentCooldown, CardModifierContext context) => currentCooldown;
        public virtual void OnBeforeCardUse(CardModifierContext context) { }
        public virtual void OnAfterCardUse(CardModifierContext context) { }
        public virtual void OnCooldownReady(CardModifierContext context) { }
    }
}
