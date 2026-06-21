using TestCardGame.Character;
using TestCardGame.Character.StatusEffects;
using TestCardGame.Controller.Services;
using UnityEngine;

namespace TestCardGame.Definitions.StatusEffects
{
    [CreateAssetMenu(fileName = "PoisonStatusEffect", menuName = "Card Game/Status Effects/Poison")]
    public class PoisonStatusEffectSO : StatusEffectSO
    {
        public override void OnTurnEnd(IUnit unit, StatusEffectInstance instance, StatusEffectService service)
        {
            if (instance.RemainingTurns > 0)
            {
                service.DamageService.DealDamage(null, unit, instance.Value, DamageType.Poison);
                instance.RemainingTurns--;
            }
        }
    }
}
