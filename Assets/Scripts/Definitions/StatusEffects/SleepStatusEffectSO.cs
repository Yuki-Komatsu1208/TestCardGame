using TestCardGame.Character;
using TestCardGame.Character.StatusEffects;
using TestCardGame.Controller.Services;
using UnityEngine;

namespace TestCardGame.Definitions.StatusEffects
{
    [CreateAssetMenu(fileName = "SleepStatusEffect", menuName = "Card Game/Status Effects/Sleep")]
    public class SleepStatusEffectSO : StatusEffectSO
    {
        /// <summary>
        /// 睡眠中は行動不能として扱う。
        /// </summary>
        public override bool CanAct(IUnit unit, StatusEffectInstance instance)
        {
            return false;
        }

        /// <summary>
        /// ターン終了時に睡眠の残りターンを減らす。
        /// </summary>
        public override void OnTurnEnd(IUnit unit, StatusEffectInstance instance, StatusEffectService service)
        {
            if (instance.RemainingTurns > 0)
            {
                instance.RemainingTurns--;
                Debug.Log($"{unit.Name}はおやすみしています（睡眠残り: {instance.RemainingTurns}ターン）。");
            }
        }
    }
}
