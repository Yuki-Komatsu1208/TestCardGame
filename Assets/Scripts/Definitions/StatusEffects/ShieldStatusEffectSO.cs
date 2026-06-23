using TestCardGame.Character;
using TestCardGame.Character.StatusEffects;
using TestCardGame.Controller.Services;
using UnityEngine;

namespace TestCardGame.Definitions.StatusEffects
{
    [CreateAssetMenu(fileName = "ShieldStatusEffect", menuName = "Card Game/Status Effects/Shield")]
    public class ShieldStatusEffectSO : StatusEffectSO
    {
        public override void OnTurnEnd(IUnit unit, StatusEffectInstance instance, StatusEffectService service)
        {
            if (instance.RemainingTurns > 0)
            {
                instance.RemainingTurns--;
            }
        }

        public override void Merge(StatusEffectInstance current, StatusEffectInstance incoming)
        {
            current.RemainingTurns = Mathf.Max(current.RemainingTurns, incoming.RemainingTurns);
            current.Value += incoming.Value;
        }
    }
}
