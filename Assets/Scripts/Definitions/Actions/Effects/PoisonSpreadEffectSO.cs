using TestCardGame.Definitions.StatusEffects;
using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 毒拡散効果の設定を持つアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewPoisonSpreadEffect", menuName = "Card Game/Effects/Poison Spread")]
    public class PoisonSpreadEffectSO : ActionEffectSO
    {
        [SerializeField] private PoisonStatusEffectSO poisonDefinition;
        [SerializeField, Min(1)] private int duration = 3;
        [SerializeField, Min(1)] private int value = 2;

        /// <summary>
        /// 毒の定義と付与量をまとめて更新する。
        /// </summary>
        public void Configure(PoisonStatusEffectSO definition, int duration, int value)
        {
            poisonDefinition = definition;
            this.duration = duration;
            this.value = value;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            return new ePoisonSpread(poisonDefinition, duration, value);
        }
    }
}
