using UnityEngine;

namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// カード使用後、対象ユニットへ追加ダメージを与えるModifier。
    /// </summary>
    public sealed class BonusDamageModifier : CardModifier
    {
        private readonly int damage;
        private readonly bool requireTargetStatusEffect;

        public BonusDamageModifier(int damage, bool requireTargetStatusEffect)
        {
            this.damage = Mathf.Max(0, damage);
            this.requireTargetStatusEffect = requireTargetStatusEffect;
        }

        public override void OnAfterCardUse(CardModifierContext context)
        {
            var target = context?.TargetUnit;
            if (target == null || damage <= 0)
            {
                return;
            }

            if (requireTargetStatusEffect && (target.StatusEffects == null || target.StatusEffects.Count == 0))
            {
                return;
            }

            target.Hp.TakeDamage(damage);
        }
    }
}
