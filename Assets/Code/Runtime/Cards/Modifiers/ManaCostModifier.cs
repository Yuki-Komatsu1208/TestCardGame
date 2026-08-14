using TestCardGame.Cards.VOs;

namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// カード使用時のマナコストを増減するModifier。
    /// </summary>
    public sealed class ManaCostModifier : CardModifier
    {
        private readonly int delta;

        public ManaCostModifier(int delta)
        {
            this.delta = delta;
        }

        public override ManaCost ModifyCost(ManaCost currentCost, CardModifierContext context)
        {
            if (delta >= 0)
            {
                return currentCost.IncreaseBy(delta);
            }

            return currentCost.DecreaseBy(-delta);
        }
    }
}
