using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// 力上昇（パワー）付与効果の設定アセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewGainPowerEffect", menuName = "Card Game/Effects/Gain Power")]
    public class GainPowerEffectSO : ActionEffectSO
    {
        [SerializeField, Min(1)] private int defaultAmount = 3;

        public override string[] ParameterFields => new[] { "value" };

        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.value = defaultAmount;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int amount = Mathf.Max(1, parameters?.value ?? defaults.value);
            return new eGainPower(amount);
        }
    }
}