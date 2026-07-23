using TestCardGame.Cards.Modifiers;
using UnityEngine;

namespace TestCardGame.Cards.Core.Modifiers
{
    [CreateAssetMenu(fileName = "InstantModifier", menuName = "Card Game/Card Modifiers/Instant")]
    public sealed class InstantModifierSO : CardModifierSO
    {
        public override CardModifier CreateRuntimeModifier()
        {
            return new InstantModifier();
        }
    }
}
