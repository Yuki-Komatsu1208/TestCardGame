using TestCardGame.Cards.Modifiers;
using UnityEngine;

namespace TestCardGame.Cards.Core.Modifiers
{
    [CreateAssetMenu(fileName = "NewManaCostModifier", menuName = "Card Game/Card Modifiers/Mana Cost")]
    public sealed class ManaCostModifierSO : CardModifierSO
    {
        [SerializeField] private int costDelta = -1;

        public override CardModifier CreateRuntimeModifier()
        {
            return new ManaCostModifier(costDelta);
        }
    }
}
