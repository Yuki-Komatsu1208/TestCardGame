using TestCardGame.Character;
using TestCardGame.Character.StatusEffects;
using TestCardGame.Controller.Services;
using UnityEngine;

namespace TestCardGame.Definitions.StatusEffects
{
    [CreateAssetMenu(fileName = "WeakStatusEffect", menuName = "Card Game/Status Effects/Weak")]
    public class WeakStatusEffectSO : StatusEffectSO
    {
        public override void OnTurnEnd(IUnit unit, StatusEffectInstance instance, StatusEffectService service)
        {
            if (instance.RemainingTurns > 0)
            {
                instance.RemainingTurns--;
                Debug.Log($"{unit.Name}は弱体化しています（弱体化残り: {instance.RemainingTurns}ターン）。");
            }
        }
    }
}