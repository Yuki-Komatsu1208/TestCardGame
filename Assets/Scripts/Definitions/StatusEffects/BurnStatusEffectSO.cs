using TestCardGame.Character;
using TestCardGame.Character.StatusEffects;
using TestCardGame.Controller.Services;
using UnityEngine;

namespace TestCardGame.Definitions.StatusEffects
{
    [CreateAssetMenu(fileName = "BurnStatusEffect", menuName = "Card Game/Status Effects/Burn")]
    public class BurnStatusEffectSO : StatusEffectSO
    {
        public override void OnTurnEnd(IUnit unit, StatusEffectInstance instance, StatusEffectService service)
        {
            if (instance.RemainingTurns > 0)
            {
                service.DamageService.DealDamage(null, unit, instance.Value, DamageType.Fire);
                instance.RemainingTurns--;
            }
        }
    }
}
