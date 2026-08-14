using TestCardGame.Cards.VOs;

namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// カード使用後に開始するクールタイムを増減するModifier。
    /// </summary>
    public sealed class CooldownModifier : CardModifier
    {
        private readonly int delta;

        public CooldownModifier(int delta)
        {
            this.delta = delta;
        }

        public override CardCooldown ModifyCooldown(CardCooldown currentCooldown, CardModifierContext context)
        {
            if (delta >= 0)
            {
                return currentCooldown.IncreaseBy(delta);
            }

            return currentCooldown.DecreaseBy(-delta);
        }
    }
}
