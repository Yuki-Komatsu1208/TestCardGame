using UnityEngine;

namespace TestCardGame.Actions.Effects
{
    /// <summary>
    /// HPを減らさず、対象のスタン耐性だけを減らす効果の設定アセット。
    /// </summary>
    [CreateAssetMenu(fileName = "NewStunDamageEffect", menuName = "Card Game/Effects/Stun Damage")]
    public sealed class StunDamageEffectSO : ActionEffectSO
    {
        [SerializeField, Min(0)] private int defaultStunDamage = 10;

        public override string[] ParameterFields => new[] { "StunDamage" };

        public override void SetDefaultParameters(ActionEffectParameters parameters)
        {
            parameters.StunDamage = defaultStunDamage;
        }

        public override ActionEffect CreateRuntimeEffect(ActionEffectParameters parameters, int level = 1)
        {
            var defaults = CreateDefaultParameters(level);
            int stunDamage = Mathf.Max(0, parameters?.StunDamage ?? defaults.StunDamage);
            return new eStunDamage(stunDamage);
        }
    }
}
