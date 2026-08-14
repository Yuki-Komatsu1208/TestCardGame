using TestCardGame.Cards.Modifiers;
using UnityEngine;

namespace TestCardGame.Cards.Core.Modifiers
{
    [CreateAssetMenu(fileName = "NewBonusDamageModifier", menuName = "Card Game/Card Modifiers/Bonus Damage")]
    public sealed class BonusDamageModifierSO : CardModifierSO
    {
        [SerializeField, Min(0)] private int damage = 5;
        [SerializeField] private bool requireTargetStatusEffect;

        public override CardModifier CreateRuntimeModifier()
        {
            return new BonusDamageModifier(damage, requireTargetStatusEffect);
        }
    }
}
