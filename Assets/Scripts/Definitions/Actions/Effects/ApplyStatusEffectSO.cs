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

        /// <summary>
        /// レベルに応じた持続ターンと値を持つ状態異常付与効果を作成する。
        /// </summary>
        public override ActionEffect CreateRuntimeEffect(int level = 1)
        {
            int duration = baseDuration + (level - 1);
            int value = baseValue * level;
            return new eApplyStatus(statusEffect, duration, value);
        }
    }
}
