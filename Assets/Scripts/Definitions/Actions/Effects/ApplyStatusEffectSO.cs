using UnityEngine;
using TestCardGame.Definitions.StatusEffects;

namespace TestCardGame.Actions.Effects
{
    [CreateAssetMenu(fileName = "NewApplyStatusEffect", menuName = "Card Game/Effects/Apply Status Effect")]
    public class ApplyStatusEffectSO : ActionEffectSO
    {
        [SerializeField] private StatusEffectSO statusEffect;
        [SerializeField] private int baseDuration = 1;
        [SerializeField] private int baseValue = 0;

        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            int duration = baseDuration + (level - 1);
            int value = baseValue * level;
            return new eApplyStatus(statusEffect, duration, value);
        }
    }
}