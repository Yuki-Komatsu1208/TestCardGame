using System.Collections.Generic;
using UnityEngine;

namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// カード使用後、別カードのクールタイムを短縮するModifier。
    /// </summary>
    public sealed class MaintenanceModifier : CardModifier
    {
        private readonly int cooldownReduction;

        public MaintenanceModifier(int cooldownReduction)
        {
            this.cooldownReduction = Mathf.Max(0, cooldownReduction);
        }

        public override void OnAfterCardUse(CardModifierContext context)
        {
            if (context?.Player?.Cards == null || cooldownReduction <= 0)
            {
                return;
            }

            var candidates = new List<Core.CardBase>();
            foreach (var card in context.Player.Cards)
            {
                if (card != null && !ReferenceEquals(card, context.Card) && card.IsCoolingDown)
                {
                    candidates.Add(card);
                }
            }

            if (candidates.Count == 0)
            {
                return;
            }

            var target = candidates[Random.Range(0, candidates.Count)];
            var wasCoolingDown = target.IsCoolingDown;
            for (var i = 0; i < cooldownReduction; i++)
            {
                target.TickCooldown();
            }

            if (wasCoolingDown && !target.IsCoolingDown)
            {
                target.OnCooldownReady(new CardModifierContext(target, context.Player));
            }
        }
    }
}
