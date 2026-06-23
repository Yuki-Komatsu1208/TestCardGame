using TestCardGame.Cards.Modifiers;
using UnityEngine;

namespace TestCardGame.Cards.Core.Modifiers
{
    [CreateAssetMenu(fileName = "NewHealOnCooldownReadyModifier", menuName = "Card Game/Card Modifiers/Heal On Cooldown Ready")]
    public sealed class HealOnCooldownReadyModifierSO : CardModifierSO
    {
        [SerializeField, Min(0)] private int healAmount = 3;

        public override CardModifier CreateRuntimeModifier()
        {
            return new HealOnCooldownReadyModifier(healAmount);
        }
    }
}
