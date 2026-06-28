using UnityEngine;
using TestCardGame.Character.StatusEffects;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 状態異常付与効果の設定を持つアセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewApplyStatusEffect", menuName = "Card Game/Effects/Apply Status Effect")]
    public class ApplyStatusEffectSO : ActionEffectSO
    {
        [SerializeField] private StatusEffectId defaultStatusEffect = StatusEffectId.Sleep;
        [SerializeField, Min(1)] private int defaultDuration = 1;
        [SerializeField, Min(0)] private int defaultValue;

        public override string[] ParameterFields => new[] { "statusEffect", "duration", "value" };

        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.statusEffect = defaultStatusEffect;
            parameters.duration = defaultDuration;
            parameters.value = defaultValue;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int duration = Mathf.Max(1, parameters?.duration ?? defaults.duration);
            int value = Mathf.Max(0, parameters?.value ?? defaults.value);
            var targetStatus = parameters != null && parameters.statusEffect != StatusEffectId.None
                ? parameters.statusEffect
                : defaults.statusEffect;
            return new eApplyStatus(targetStatus, duration, value);
        }
    }
}
