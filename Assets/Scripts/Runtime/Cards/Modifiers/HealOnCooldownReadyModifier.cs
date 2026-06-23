using UnityEngine;

namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// カードの残りクールタイムが0になった時、プレイヤーを回復するModifier。
    /// </summary>
    public sealed class HealOnCooldownReadyModifier : CardModifier
    {
        private readonly int healAmount;

        public HealOnCooldownReadyModifier(int healAmount)
        {
            this.healAmount = Mathf.Max(0, healAmount);
        }

        public override void OnCooldownReady(CardModifierContext context)
        {
            context?.Player?.Hp.Heal(healAmount);
        }
    }
}
