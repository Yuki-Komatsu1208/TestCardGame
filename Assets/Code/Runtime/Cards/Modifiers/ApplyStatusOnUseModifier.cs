using TestCardGame.Character.StatusEffects;
using UnityEngine;

namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// カード使用後、対象ユニットへ状態異常を付与するModifier。
    /// </summary>
    public sealed class ApplyStatusOnUseModifier : CardModifier
    {
        private readonly StatusEffectId statusEffect;
        private readonly int duration;
        private readonly int value;

        public ApplyStatusOnUseModifier(StatusEffectId statusEffect, int duration, int value)
        {
            this.statusEffect = statusEffect;
            this.duration = Mathf.Max(1, duration);
            this.value = Mathf.Max(0, value);
        }

        public override void OnAfterCardUse(CardModifierContext context)
        {
            var target = context?.TargetUnit;
            if (target == null || statusEffect == StatusEffectId.None)
            {
                return;
            }

            context.ActionContext?.StatusEffectService?.ApplyStatus(target, statusEffect, duration, value);
        }
    }
}
