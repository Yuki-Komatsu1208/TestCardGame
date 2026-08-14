using TestCardGame.Cards.Modifiers;
using UnityEngine;

namespace TestCardGame.Cards.Core.Modifiers
{
    [CreateAssetMenu(fileName = "NewCooldownModifier", menuName = "Card Game/Card Modifiers/Cooldown")]
    public sealed class CooldownModifierSO : CardModifierSO
    {
        [SerializeField] private int cooldownDelta = -1;

        public override CardModifier CreateRuntimeModifier()
        {
            return new CooldownModifier(cooldownDelta);
        }
    }
}
