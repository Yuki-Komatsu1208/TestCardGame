using TestCardGame.Cards.Modifiers;
using UnityEngine;

namespace TestCardGame.Cards.Core.Modifiers
{
    [CreateAssetMenu(fileName = "NewOverheatModifier", menuName = "Card Game/Card Modifiers/Overheat")]
    public sealed class OverheatModifierSO : CardModifierSO
    {
        [SerializeField, Min(0)] private int damage = 8;
        [SerializeField, Min(0)] private int cooldownIncrease = 1;

        public override CardModifier CreateRuntimeModifier()
        {
            return new OverheatModifier(damage, cooldownIncrease);
        }
    }
}
