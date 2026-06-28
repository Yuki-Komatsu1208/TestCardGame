using TestCardGame.Cards.Modifiers;
using TestCardGame.Character.StatusEffects;
using UnityEngine;

namespace TestCardGame.Cards.Core.Modifiers
{
    [CreateAssetMenu(fileName = "NewApplyStatusOnUseModifier", menuName = "Card Game/Card Modifiers/Apply Status On Use")]
    public sealed class ApplyStatusOnUseModifierSO : CardModifierSO
    {
        [SerializeField] private StatusEffectId statusEffect = StatusEffectId.Burn;
        [SerializeField, Min(1)] private int duration = 1;
        [SerializeField, Min(0)] private int value;

        public override CardModifier CreateRuntimeModifier()
        {
            return new ApplyStatusOnUseModifier(statusEffect, duration, value);
        }
    }
}
