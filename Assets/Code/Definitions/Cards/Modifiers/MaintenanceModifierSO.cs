using TestCardGame.Cards.Modifiers;
using UnityEngine;

namespace TestCardGame.Cards.Core.Modifiers
{
    [CreateAssetMenu(fileName = "NewMaintenanceModifier", menuName = "Card Game/Card Modifiers/Maintenance")]
    public sealed class MaintenanceModifierSO : CardModifierSO
    {
        [SerializeField, Min(0)] private int cooldownReduction = 1;

        public override CardModifier CreateRuntimeModifier()
        {
            return new MaintenanceModifier(cooldownReduction);
        }
    }
}
