using TestCardGame.Cards.VOs;
using UnityEngine;

namespace TestCardGame.Cards.Modifiers
{
    /// <summary>
    /// 追加ダメージを与える代わりに使用後クールタイムを伸ばすModifier。
    /// </summary>
    public sealed class OverheatModifier : CardModifier
    {
        private readonly int damage;
        private readonly int cooldownIncrease;

        public OverheatModifier(int damage, int cooldownIncrease)
        {
            this.damage = Mathf.Max(0, damage);
            this.cooldownIncrease = Mathf.Max(0, cooldownIncrease);
        }

        public override CardCooldown ModifyCooldown(CardCooldown currentCooldown, CardModifierContext context)
        {
            return currentCooldown.IncreaseBy(cooldownIncrease);
        }

        public override void OnAfterCardUse(CardModifierContext context)
        {
            var target = context?.TargetUnit;
            if (target == null || damage <= 0)
            {
                return;
            }

            target.Hp.TakeDamage(damage);
        }
    }
}
